using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.TextManager.Interop;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;

using Shell =  Microsoft.VisualStudio.Shell;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;


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

		/// <summary>
		/// Initializes a new instance of the <see cref="GoToDeclarationOrHandlerCommand"/> class. Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">The command service.</param>
		private GoToDeclarationOrHandlerCommand(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService) :
										   base(package, commandService, GoToDeclarationCommandId)
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
		/// Initializes the singleton instance of the command. Internal method shich should be called only from UI thread.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="oleCommandService">The OLE command service.</param>
		/// <returns/>
		internal static void Initialize(Shell.AsyncPackage package, Shell.OleMenuCommandService oleCommandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new GoToDeclarationOrHandlerCommand(package, oleCommandService);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e) =>
			CommandCallbackAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(GoToDeclarationOrHandlerCommand)}");
		
		private async Task CommandCallbackAsync()
		{
			IWpfTextView textView = await ServiceProvider.GetWpfTextViewAsync();

			if (textView == null || Package.DisposalToken.IsCancellationRequested)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			ITextSnapshotLine caretLine = caretPosition.GetContainingLine();

			if (caretLine == null)
				return;

			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null || Package.DisposalToken.IsCancellationRequested)
				return;

			Task<SyntaxNode> syntaxRootTask = document.GetSyntaxRootAsync();
			Task<SemanticModel> semanticModelTask = document.GetSemanticModelAsync();
			await Task.WhenAll(syntaxRootTask, semanticModelTask);

			#pragma warning disable VSTHRD002, VSTHRD103 // Avoid problematic synchronous waits - the results are already obtained
			SyntaxNode syntaxRoot = syntaxRootTask.Result;
			SemanticModel semanticModel = semanticModelTask.Result;
			#pragma warning restore VSTHRD002, VSTHRD103

			if (syntaxRoot == null || semanticModel == null)
				return;

			TextSpan lineSpan = TextSpan.FromBounds(caretLine.Start.Position, caretLine.End.Position);

			if (!(syntaxRoot.FindNode(lineSpan) is MemberDeclarationSyntax memberNode))
				return;

			PXContext context = new PXContext(semanticModel.Compilation, Acuminator.Utilities.CodeAnalysisSettings.Default);

			if (!context.IsPlatformReferenced || Package.DisposalToken.IsCancellationRequested)
				return;

			ISymbol memberSymbol = GetMemberSymbol(memberNode, semanticModel, caretPosition);

			if (!CheckMemberSymbol(memberSymbol, context))
				return;

			await NavigateToHandlerOrDeclarationAsync(document, textView, memberSymbol, memberNode, semanticModel, context);
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

		private async Task NavigateToHandlerOrDeclarationAsync(Document document, IWpfTextView textView, ISymbol memberSymbol,
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
					await NavigateToPXActionHandlerAsync(document, textView, fieldSymbol, graphSemanticModel, context);
					return;
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsBqlCommand(context):
					await NavigateToPXViewDelegateAsync(document, textView, fieldSymbol, graphSemanticModel, context);
					return;								
				case IMethodSymbol methodSymbol:
					await NavigateToActionOrViewDeclarationAsync(document, textView, methodSymbol, graphSemanticModel, context);
					return;
			}
		}

		private async Task NavigateToPXActionHandlerAsync(Document document, IWpfTextView textView, ISymbol actionSymbol,
														  PXGraphSemanticModel graphSemanticModel, PXContext context)
		{
			if (!graphSemanticModel.ActionHandlersByNames.TryGetValue(actionSymbol.Name, out ActionHandlerInfo actionHandler))
				return;

			IWpfTextView textViewToNavigateTo = textView;

			if (!(actionHandler.Node is MethodDeclarationSyntax handlerNode) || handlerNode.SyntaxTree == null)
				return;

			if (handlerNode.SyntaxTree.FilePath != document.FilePath)
			{
				textViewToNavigateTo = await OpenOtherDocumentForNavigationAndGetItsTextViewAsync(document, handlerNode.SyntaxTree);
			}	

			if (textViewToNavigateTo == null)
				return;

			await SetNewPositionInTextViewAsync(textViewToNavigateTo, handlerNode.Identifier.Span);
		}	
										
		private async Task NavigateToPXViewDelegateAsync(Document document, IWpfTextView textView, ISymbol viewSymbol, 
														 PXGraphSemanticModel graphSemanticModel, PXContext context)
		{
			if (!graphSemanticModel.ViewDelegatesByNames.TryGetValue(viewSymbol.Name, out var viewDelegate))
				return;

			var viewDelegates = viewDelegate.GetDelegateWithAllOverrides().ToList();

			DataViewDelegateInfo viewDelegateInfoToNavigateTo;

			if (viewDelegates.Count == 1)
			{
				viewDelegateInfoToNavigateTo = viewDelegates.First();
			}
			else
			{
				viewDelegateInfoToNavigateTo = viewDelegates.Where(vDelegate => vDelegate.Symbol.ContainingType == viewSymbol.ContainingType)
															.FirstOrDefault();
			}

			if (!(viewDelegateInfoToNavigateTo.Node is MethodDeclarationSyntax methodNode) || methodNode.SyntaxTree == null)
				return;

			string viewDelegateFilePath = viewDelegateInfoToNavigateTo.Node.SyntaxTree.FilePath;
			IWpfTextView textViewToNavigateTo = textView;

			if (viewDelegateFilePath != document.FilePath)
			{
				textViewToNavigateTo = await OpenOtherDocumentForNavigationAndGetItsTextViewAsync(document,
																								  viewDelegateInfoToNavigateTo.Node.SyntaxTree);
			}

			if (textViewToNavigateTo == null)
				return;

			await SetNewPositionInTextViewAsync(textViewToNavigateTo, methodNode.Identifier.Span);
		}

		private async Task NavigateToActionOrViewDeclarationAsync(Document document, IWpfTextView textView, IMethodSymbol methodSymbol, 
																  PXGraphSemanticModel graphSemanticModel, PXContext context)
		{
			ISymbol symbolToNavigate = GetActionOrViewSymbolToNavigateTo(methodSymbol, graphSemanticModel, context);

			if (symbolToNavigate == null || Package.DisposalToken.IsCancellationRequested)
				return;

			ImmutableArray<SyntaxReference> syntaxReferences = symbolToNavigate.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return;

			SyntaxReference syntaxReference = syntaxReferences[0];

			if (syntaxReference.SyntaxTree.FilePath != document.FilePath)
			{
				textView = await OpenOtherDocumentForNavigationAndGetItsTextViewAsync(document, syntaxReference.SyntaxTree);
			}

			if (textView == null)
				return;

			SyntaxNode declarationNode = await syntaxReference.GetSyntaxAsync(Package.DisposalToken);
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

			await SetNewPositionInTextViewAsync(textView, textSpan);
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

		private async Task<IWpfTextView> OpenOtherDocumentForNavigationAndGetItsTextViewAsync(Document originalDocument, SyntaxTree syntaxTreeToNavigate)
		{
			DocumentId documentToNavigateId = originalDocument.Project.GetDocumentId(syntaxTreeToNavigate);

			if (documentToNavigateId == null)
				return null;

			bool wasAlreadyOpened = originalDocument.Project.Solution.Workspace.IsDocumentOpen(documentToNavigateId);
			originalDocument.Project.Solution.Workspace.OpenDocument(documentToNavigateId);

			if (!wasAlreadyOpened)
			{		
				return await ServiceProvider.GetWpfTextViewAsync();
			}

			var documentToNavigate = originalDocument.Project.GetDocument(documentToNavigateId);

			if (documentToNavigate == null)
				return null;

			var wpfTextView = await ServiceProvider.GetWpfTextViewByFilePathAsync(documentToNavigate.FilePath);
			return wpfTextView;
		}

		private async Task SetNewPositionInTextViewAsync(IWpfTextView textView, TextSpan textSpan)
		{
			SnapshotSpan selectedSpan = new SnapshotSpan(textView.TextSnapshot, textSpan.Start, textSpan.Length);

		 	await ExpandAllRegionsContainingSpanAsync(selectedSpan, textView);

			textView.MoveCaretTo(textSpan.Start);                      
			textView.ViewScroller.EnsureSpanVisible(selectedSpan, EnsureSpanVisibleOptions.AlwaysCenter);			
			textView.Selection.Select(selectedSpan, isReversed: false);
		}

		private async Task ExpandAllRegionsContainingSpanAsync(SnapshotSpan selectedSpan, IWpfTextView textView)
		{
			IOutliningManager outliningManager = await ServiceProvider.GetOutliningManagerAsync(textView);

			if (outliningManager == null)
				return;

			outliningManager.GetCollapsedRegions(selectedSpan, exposedRegionsOnly: false)
							.ForEach(region => outliningManager.Expand(region));
		}
	}
}
