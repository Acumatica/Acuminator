using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using EnvDTE;
using EnvDTE80;
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
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;


namespace Acuminator.Vsix.GoToDeclaration
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class GoToDeclarationOrHandlerCommand : VSCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Gro to View/Declaration Command ID.
		/// </summary>
		public const int GoToDeclarationCommandId = 0x0102;

		protected override bool CanModifyDocument => false;

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
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
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
			if (document == null) return;

			(SyntaxNode syntaxRoot, SemanticModel semanticModel) = ThreadHelper.JoinableTaskFactory.Run(
				async () => (await document.GetSyntaxRootAsync(), await document.GetSemanticModelAsync()));

			if (syntaxRoot == null || semanticModel == null)
				return;

			TextSpan lineSpan = TextSpan.FromBounds(caretLine.Start.Position, caretLine.End.Position);
			var memberNode = syntaxRoot.FindNode(lineSpan) as MemberDeclarationSyntax;  

			if (memberNode == null)
				return;

			PXContext context = new PXContext(semanticModel.Compilation);

			if (!context.IsPlatformReferenced)
				return;

			ISymbol memberSymbol = GetMemberSymbol(memberNode, semanticModel, caretPosition);

			if (!CheckMemberSymbol(memberSymbol, context))
				return;

			NavigateToHandlerOrDeclaration(document, textView, memberSymbol, memberNode, semanticModel, context);
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

		private bool CheckMemberSymbol(ISymbol memberSymbol, PXContext context)
		{
			if (memberSymbol?.ContainingType == null || !memberSymbol.ContainingType.IsPXGraphOrExtension(context))
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
													MemberDeclarationSyntax memberNode, SemanticModel semanticModel, PXContext context)
		{
			INamedTypeSymbol graphOrExtensionType = memberSymbol.ContainingType;
			PXGraphSemanticModel graphSemanticModel = PXGraphSemanticModel.InferModels(context, graphOrExtensionType)
																		 ?.FirstOrDefault();

			if (graphSemanticModel == null || graphSemanticModel.Type == GraphType.None)
				return;

			switch (memberSymbol)
			{
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsPXAction():
					NavigateToPXActionHandler(document, textView, fieldSymbol, graphSemanticModel, context);
					return;
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsBqlCommand(context):
					NavigateToPXViewDelegate(document, textView, fieldSymbol, graphSemanticModel, context);
					return;								
				case IMethodSymbol methodSymbol:
					NavigateToActionOrViewDeclaration(document, textView, methodSymbol, graphSemanticModel, context);
					return;
			}
		}

		private void NavigateToPXActionHandler(Document document, IWpfTextView textView, ISymbol actionSymbol, PXGraphSemanticModel graphSemanticModel,
											   PXContext context)
		{
			if (!graphSemanticModel.ActionHandlersByNames.TryGetValue(actionSymbol.Name, out ActionHandlerInfo actionHandler))
				return;

			IWpfTextView textViewToNavigateTo = textView;

			if (!(actionHandler.Node is MethodDeclarationSyntax handlerNode) || handlerNode.SyntaxTree == null)
				return;

			if (handlerNode.SyntaxTree.FilePath != document.FilePath)
			{
				textViewToNavigateTo = OpenOtherDocumentForNavigationAndGetItsTextView(document, handlerNode.SyntaxTree);
			}	

			if (textViewToNavigateTo == null)
				return;

			SetNewPositionInTextView(textViewToNavigateTo, handlerNode.Identifier.Span);
		}	
										
		private void NavigateToPXViewDelegate(Document document, IWpfTextView textView, ISymbol viewSymbol, 
											  PXGraphSemanticModel graphSemanticModel, PXContext context)
		{		
			var viewDelegates = graphSemanticModel.ViewDelegates
												  .Where(vDelegate => string.Equals(viewSymbol.Name, vDelegate.Symbol.Name,
																					StringComparison.OrdinalIgnoreCase))
												  .ToList();
			if (viewDelegates.Count == 0)
				return;

			DataViewDelegateInfo viewDelegateInfo;

			if (viewDelegates.Count == 1)
			{
				viewDelegateInfo = viewDelegates.First();
			}
			else
			{
				viewDelegateInfo = viewDelegates.Where(vDelegate => vDelegate.Symbol.ContainingType == viewSymbol.ContainingType)
												.FirstOrDefault();
			}

			if (!(viewDelegateInfo.Node is MethodDeclarationSyntax methodNode) || methodNode.SyntaxTree == null)
				return;

			string viewDelegateFilePath = viewDelegateInfo.Node.SyntaxTree.FilePath;
			IWpfTextView textViewToNavigateTo = textView;

			if (viewDelegateFilePath != document.FilePath)
			{
				textViewToNavigateTo = OpenOtherDocumentForNavigationAndGetItsTextView(document, viewDelegateInfo.Node.SyntaxTree);
			}

			if (textViewToNavigateTo == null)
				return;

			SetNewPositionInTextView(textViewToNavigateTo, methodNode.Identifier.Span);
		}

		private void NavigateToActionOrViewDeclaration(Document document, IWpfTextView textView, IMethodSymbol methodSymbol, 
													   PXGraphSemanticModel graphSemanticModel, PXContext context)
		{
			ISymbol symbolToNavigate = GetActionOrViewSymbolToNavigateTo(methodSymbol, graphSemanticModel, context);

			if (symbolToNavigate == null)
				return;

			ImmutableArray<SyntaxReference> syntaxReferences = symbolToNavigate.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return;

			SyntaxReference syntaxReference = syntaxReferences[0];

			if (syntaxReference.SyntaxTree.FilePath != document.FilePath)
			{
				textView = OpenOtherDocumentForNavigationAndGetItsTextView(document, syntaxReference.SyntaxTree);
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
		}

		private static ISymbol GetActionOrViewSymbolToNavigateTo(IMethodSymbol methodSymbol, PXGraphSemanticModel graphSemanticModel,
																 PXContext context)
		{
			IEnumerable<ISymbol> candidates;

			if (methodSymbol.IsValidActionHandler(context))
			{
				candidates = from action in graphSemanticModel.Actions
							 where string.Equals(action.Symbol.Name, methodSymbol.Name, StringComparison.OrdinalIgnoreCase)
							 select action.Symbol;
			}
			else if (methodSymbol.IsValidViewDelegate(context))
			{
				candidates = from viewInfo in graphSemanticModel.Views
							 where string.Equals(viewInfo.Symbol.Name, methodSymbol.Name, StringComparison.OrdinalIgnoreCase)
							 select viewInfo.Symbol;
			}
			else
				return null;

			if (candidates.IsSingle())
			{
				return candidates.First();
			}
			else
			{
				return candidates.Where(symbol => symbol.ContainingType == methodSymbol.ContainingType ||
												  symbol.ContainingType.OriginalDefinition == methodSymbol.ContainingType.OriginalDefinition)
								 .FirstOrDefault();
			}
		}

		private IWpfTextView OpenOtherDocumentForNavigationAndGetItsTextView(Document originalDocument, SyntaxTree syntaxTreeToNavigate)
		{
			DocumentId documentToNavigateId = originalDocument.Project.GetDocumentId(syntaxTreeToNavigate);

			if (documentToNavigateId == null)
				return null;

			bool wasAlreadyOpened = originalDocument.Project.Solution.Workspace.IsDocumentOpen(documentToNavigateId);
			originalDocument.Project.Solution.Workspace.OpenDocument(documentToNavigateId);

			if (!wasAlreadyOpened)
			{		
				return ServiceProvider.GetWpfTextView();
			}

			var documentToNavigate = originalDocument.Project.GetDocument(documentToNavigateId);

			if (documentToNavigate == null)
				return null;

			var wpfTextView = ServiceProvider.GetWpfTextViewByFilePath(documentToNavigate.FilePath);
			return wpfTextView;
		}

		private void SetNewPositionInTextView(IWpfTextView textView, TextSpan textSpan)
		{
			SnapshotSpan selectedSpan = new SnapshotSpan(textView.TextSnapshot, textSpan.Start, textSpan.Length);
			ExpandAllRegionsContainingSpan(selectedSpan, textView);
			CaretPosition newCaretPosition = textView.MoveCaretTo(textSpan.Start);                      

			if (!textView.TextViewLines.ContainsBufferPosition(newCaretPosition.BufferPosition))
			{
				textView.ViewScroller.EnsureSpanVisible(selectedSpan, EnsureSpanVisibleOptions.AlwaysCenter);		
			}

			textView.Selection.Select(selectedSpan, isReversed: false);
		}

		private void ExpandAllRegionsContainingSpan(SnapshotSpan selectedSpan, IWpfTextView textView)
		{
			IOutliningManager outliningManager = ServiceProvider.GetOutliningManager(textView);

			if (outliningManager == null)
				return;

			outliningManager.GetCollapsedRegions(selectedSpan, exposedRegionsOnly: false)
							.ForEach(region => outliningManager.Expand(region));
		}
	}
}
