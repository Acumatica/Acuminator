using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Acuminator.Utilities;
using Acuminator.Analyzers;
using Acuminator.Vsix;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Extra;


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;


namespace Acuminator.Vsix.GoToDeclaration
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class GoToDeclarationOrHandlerCommand : VSCommandBase
	{
		private static int IsCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Gro to View/Declaration Command ID.
		/// </summary>
		public const int GoToDeclarationCommandId = 0x0102;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoToDeclarationOrHandlerCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="aPackage">Owner package, not null.</param>
		private GoToDeclarationOrHandlerCommand(Package aPackage) : base(aPackage, GoToDeclarationCommandId)
		{
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static GoToDeclarationOrHandlerCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			if (Interlocked.CompareExchange(ref IsCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new GoToDeclarationOrHandlerCommand(package);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e)
		{
			IWpfTextView textView = ServiceProvider.GetWpfTextView();

			if (textView == null)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			ITextSnapshotLine caretLine = caretPosition.GetContainingLine();

			if (caretLine == null)
				return;

			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			SyntaxNode syntaxRoot = document?.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document?.GetSemanticModelAsync().Result;

			if (syntaxRoot == null || semanticModel == null)
				return;

			TextSpan lineSpan = TextSpan.FromBounds(caretLine.Start.Position, caretLine.End.Position);
			var memberNode = syntaxRoot.FindNode(lineSpan) as MemberDeclarationSyntax;  

			if (memberNode == null)
				return;

			ISymbol memberSymbol = GetMemberSymbol(memberNode, semanticModel, caretPosition);

			if (!CheckMemberSymbol(memberSymbol))
				return;

			NavigateToHandlerOrDeclaration(document, textView, memberSymbol, memberNode, semanticModel);
		}

		private ISymbol GetMemberSymbol(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, SnapshotPoint caretPosition)
		{
			if (!(memberDeclaration is FieldDeclarationSyntax fieldDeclaration))
			{
				return semanticModel.GetDeclaredSymbol(memberDeclaration);
			}

			var variableDeclaration = fieldDeclaration.Declaration.Variables
																  .FirstOrDefault(v => v.Span.Contains(caretPosition.Position));

			if (variableDeclaration == null)
				return null;

			return semanticModel.GetDeclaredSymbol(variableDeclaration);
		}

		private bool CheckMemberSymbol(ISymbol memberSymbol)
		{
			if (memberSymbol?.ContainingType == null || !memberSymbol.ContainingType.IsPXGraph())
				return false;

			switch (memberSymbol.Kind)
			{
				case SymbolKind.Field:
				case SymbolKind.Method:
				case SymbolKind.Property:
					return true;	
				default:
					return false;
			}
		}

		private void NavigateToHandlerOrDeclaration(Document document, IWpfTextView textView, ISymbol memberSymbol,
													MemberDeclarationSyntax memberNode, SemanticModel semanticModel)
		{
			PXContext context = new PXContext(semanticModel.Compilation);
			INamedTypeSymbol graphType = memberSymbol.ContainingType;
			
			switch (memberSymbol)
			{
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsPXAction():
					NavigateToPXActionHandler(document, textView, fieldSymbol, graphType, context);
					return;
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsBqlCommand(context):
					NavigateToPXViewDelegate(document, textView, fieldSymbol, graphType, context);
					return;
				case IPropertySymbol propertySymbol when propertySymbol.Type.IsPXAction():
					NavigateToPXActionHandler(document, textView, propertySymbol, graphType, context);
					return;
				case IPropertySymbol propertySymbol when propertySymbol.Type.IsBqlCommand(context):
					NavigateToPXViewDelegate(document, textView, propertySymbol, graphType, context);
					return;
				case IMethodSymbol methodSymbol:
					NavigateToActionOrViewDeclaration(document, textView, methodSymbol, graphType, context);
					return;
			}
		}

		private void NavigateToPXActionHandler(Document document, IWpfTextView textView, ISymbol actionSymbol, INamedTypeSymbol graphSymbol,
											   PXContext context)
		{
			var actionHandlers = GetActionHandlerByName(graphSymbol, actionSymbol.Name, context);

			if (!actionHandlers.IsSingle())
				return;

			IMethodSymbol actionHandler = actionHandlers.First();
			ImmutableArray<SyntaxReference> syntaxReferences = actionHandler.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return;

			SyntaxReference syntaxReference = syntaxReferences[0];

			if (syntaxReference.SyntaxTree.FilePath != document.FilePath)
			{
				textView = OpenOtherDocumentForNavigationAndGetItsTextView(document, syntaxReference);
			}

			if (textView == null)
				return;

			MethodDeclarationSyntax methodNode = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
			TextSpan textSpan = methodNode?.Identifier.Span ?? syntaxReferences[0].Span;
			SetNewPositionInTextView(textView, textSpan);
		}

		private IEnumerable<IMethodSymbol> GetActionHandlerByName(INamedTypeSymbol graphSymbol, string name, PXContext pxContext) =>
			from method in graphSymbol.GetMembers().OfType<IMethodSymbol>()
			where method.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && IsValidActionHandler(method, pxContext)
			select method;
										
		private void NavigateToPXViewDelegate(Document document, IWpfTextView textView, ISymbol viewSymbol, INamedTypeSymbol graphSymbol,
											  PXContext context)
		{
			var viewDelegates = GetDelegateByName(graphSymbol, viewSymbol.Name, context);

			if (!viewDelegates.IsSingle())
				return;
			
			IMethodSymbol viewDelegate = viewDelegates.First();
			ImmutableArray<SyntaxReference> syntaxReferences = viewDelegate.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return;

			SyntaxReference syntaxReference = syntaxReferences[0];

			if (syntaxReference.SyntaxTree.FilePath != document.FilePath)
			{
				textView = OpenOtherDocumentForNavigationAndGetItsTextView(document, syntaxReference);
			}

			if (textView == null)
				return;

			MethodDeclarationSyntax methodNode = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
			TextSpan textSpan = methodNode?.Identifier.Span ?? syntaxReferences[0].Span;
			SetNewPositionInTextView(textView, textSpan);
		}

		private void NavigateToActionOrViewDeclaration(Document document, IWpfTextView textView, IMethodSymbol methodSymbol, INamedTypeSymbol graphSymbol,
													   PXContext context)
		{
			ISymbol symbolToNavigate = GetSymbolToNavigateTo();

			if (symbolToNavigate == null)
				return;

			ImmutableArray<SyntaxReference> syntaxReferences = symbolToNavigate.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return;

			SyntaxReference syntaxReference = syntaxReferences[0];

			if (syntaxReference.SyntaxTree.FilePath != document.FilePath)
			{
				textView = OpenOtherDocumentForNavigationAndGetItsTextView(document, syntaxReference);
			}

			if (textView == null)
				return;

			SyntaxNode declarationNode = syntaxReference.GetSyntax();
			TextSpan textSpan;

			switch (declarationNode)
			{
				case VariableDeclaratorSyntax variableDeclarator:
					textSpan = variableDeclarator?.Identifier.Span ?? syntaxReferences[0].Span;
					break;
				case PropertyDeclarationSyntax propertyDeclaration:
					textSpan = propertyDeclaration?.Identifier.Span ?? syntaxReferences[0].Span;
					break;
				default:
					return;
			}

			SetNewPositionInTextView(textView, textSpan);

			//********************************************************Local Function**********************************************
			ISymbol GetSymbolToNavigateTo()
			{
				IEnumerable<ISymbol> candidates;

				if (IsValidActionHandler(methodSymbol, context))
				{
					candidates = graphSymbol.GetPXActionSymbolsWithTypesFromGraph(context, includeActionsFromInheritanceChain: false)
											.Where(actionWithType => actionWithType.ActionSymbol.Name.Equals(methodSymbol.Name,
																	 StringComparison.OrdinalIgnoreCase))
											.Select(actionWithType => actionWithType.ActionSymbol);		
				}
				else if (IsValidViewDelegate(methodSymbol, context))
				{
					candidates = graphSymbol.GetViewsWithSymbolsFromPXGraph(context, includeViewsFromInheritanceChain: false)
											.Where(viewWithType => viewWithType.ViewSymbol.Name.Equals(methodSymbol.Name,
																   StringComparison.OrdinalIgnoreCase))
											.Select(viewWithType => viewWithType.ViewSymbol);
				}
				else
					return null;

				return candidates.IsSingle()
						? candidates.First()
						: null;
			}	
		}

		private IEnumerable<IMethodSymbol> GetDelegateByName(INamedTypeSymbol graphSymbol, string name, PXContext pxContext) =>
			graphSymbol.GetMembers()
					   .OfType<IMethodSymbol>()
					   .Where(method => method.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && IsValidViewDelegate(method, pxContext));

		private IWpfTextView OpenOtherDocumentForNavigationAndGetItsTextView(Document originalDocument, SyntaxReference syntaxReference)
		{
			DocumentId documentId = originalDocument.Project.GetDocumentId(syntaxReference.SyntaxTree);

			if (documentId == null)
				return null;

			originalDocument.Project.Solution.Workspace.OpenDocument(documentId);
			return ServiceProvider.GetWpfTextView();
		}

		private void SetNewPositionInTextView(IWpfTextView textView, TextSpan textSpan)
		{
			CaretPosition newCaretPosition = textView.MoveCaretTo(textSpan.Start);
			SnapshotSpan selectedSpan = new SnapshotSpan(textView.TextSnapshot, textSpan.Start, textSpan.Length);
			textView.Selection.Select(selectedSpan, isReversed: false);

			if (!textView.TextViewLines.ContainsBufferPosition(newCaretPosition.BufferPosition))
			{
				textView.ViewScroller.EnsureSpanVisible(selectedSpan, EnsureSpanVisibleOptions.AlwaysCenter);
			}
		}

		private static bool IsValidActionHandler(IMethodSymbol method, PXContext pxContext) =>
			 method.Parameters.Length > 0 &&
			 method.Parameters[0].Type.InheritsFromOrEquals(pxContext.PXAdapterType) &&
			 method.ReturnType.InheritsFromOrEquals(pxContext.IEnumerable, includeInterfaces: true);

		private static bool IsValidViewDelegate(IMethodSymbol method, PXContext pxContext) =>
			 method.ReturnType.InheritsFromOrEquals(pxContext.IEnumerable, includeInterfaces: true);
	}
}
