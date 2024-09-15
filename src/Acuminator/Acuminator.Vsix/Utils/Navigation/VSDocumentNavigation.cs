#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Shell;

using Document = Microsoft.CodeAnalysis.Document;
using DTE = EnvDTE.DTE;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Task = System.Threading.Tasks.Task;

namespace Acuminator.Vsix.Utilities.Navigation
{
	public static class VSDocumentNavigation
	{
		public static Task<(IWpfTextView? WpfTextView, CaretPosition CaretPosition)> NavigateToAsync(this ISymbol symbol,
																									bool selectSpan = true,
																									CancellationToken cToken = default)
		{
			var syntaxReferences = symbol.CheckIfNull().DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return Task.FromResult(default((IWpfTextView?, CaretPosition)));

			return symbol.NavigateToAsync(syntaxReferences[0], selectSpan, cToken);
		}

		public static async Task<(IWpfTextView? WpfTextView, CaretPosition CaretPosition)> NavigateToAsync(this ISymbol symbol, 
																										  SyntaxReference reference, bool selectSpan = true,
																										  CancellationToken cToken = default)
		{
			symbol.ThrowOnNull();
			reference.ThrowOnNull();
			var filePath = reference.SyntaxTree?.FilePath;

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();
			TextSpan textSpanToNavigate = await GetTextSpanToNavigateFromSymbolAsync(symbol, reference, cToken);
			return await AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPositionAsync(workspace.CheckIfNull().CurrentSolution, filePath,
																							 textSpanToNavigate, selectSpan);
		}

		private static async Task<TextSpan> GetTextSpanToNavigateFromSymbolAsync(ISymbol symbol, SyntaxReference syntaxReference, 
																				 CancellationToken cToken = default)
		{
			switch (symbol)
			{
				case ITypeSymbol _:
				case IMethodSymbol _:
				case IPropertySymbol _:
				case IEventSymbol _:
					var typeMemberNode = await syntaxReference.GetSyntaxAsync(cToken) as MemberDeclarationSyntax;
					return typeMemberNode?.GetIdentifiers()
										  .FirstOrNullable()
										 ?.Span ?? syntaxReference.Span;
				default:
					return syntaxReference.Span;
			}
		}

		public static async Task<(IWpfTextView? WpfTextView, CaretPosition CaretPosition)> OpenCodeFileAndNavigateByLineAndCharAsync(
																									 this IAsyncServiceProvider serviceProvider,
																									 Solution solution, string? filePath,
																									 int lineNumber, int character)
		{
			serviceProvider.ThrowOnNull();

			if (lineNumber < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}
			else if (character < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(character));
			}

			IWpfTextView? wpfTextView = await OpenCodeWindowAsync(serviceProvider, solution, filePath);

			if (wpfTextView == null)
			{
				var window = await OpenCodeFileNotInSolutionWithDTEAsync(serviceProvider, filePath);

				if (window == null)
					return default;

				wpfTextView = await serviceProvider.GetWpfTextViewByFilePathAsync(filePath);
			}

			if (wpfTextView == null)
				return default;

			try
			{
				ITextSnapshotLine textLine = wpfTextView.TextSnapshot.GetLineFromLineNumber(lineNumber);
				int absoluteOffset = textLine.Start + character;
				SnapshotPoint point = wpfTextView.TextSnapshot.GetPoint(absoluteOffset);
				SnapshotSpan span = new SnapshotSpan(point, length: 0);

				await serviceProvider.ExpandAllRegionsContainingSpanAsync(span, wpfTextView);
				CaretPosition newCaretPosition = wpfTextView.MoveCaretTo(absoluteOffset);

				wpfTextView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
				return (wpfTextView, newCaretPosition);
			}
			catch
			{
				return default;
			}
		}

		public static async Task<(IWpfTextView? WpfTextView, CaretPosition CaretPosition)> OpenCodeFileAndNavigateToPositionAsync(
																					this IAsyncServiceProvider serviceProvider,
																					Solution solution, string? filePath, 
																					TextSpan? spanToNavigate = null,
																					bool selectSpan = true)
		{
			serviceProvider.ThrowOnNull();

			IWpfTextView? wpfTextView = await OpenCodeWindowAsync(serviceProvider, solution, filePath);

			if (wpfTextView == null)
			{
				var window = await OpenCodeFileNotInSolutionWithDTEAsync(serviceProvider, filePath);

				if (window == null)
					return default;

				wpfTextView = await serviceProvider.GetWpfTextViewByFilePathAsync(filePath);
			}

			if (wpfTextView == null)
				return default;

			try
			{
				if (spanToNavigate == null)
				{
					return (wpfTextView, wpfTextView.Caret.Position);
				}

				SnapshotPoint point = wpfTextView.TextSnapshot.GetPoint(spanToNavigate.Value.Start);
				SnapshotSpan span = new SnapshotSpan(point, spanToNavigate.Value.Length);

				await serviceProvider.ExpandAllRegionsContainingSpanAsync(span, wpfTextView);

				CaretPosition newCaretPosition = wpfTextView.MoveCaretTo(spanToNavigate.Value.Start);
				
				wpfTextView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);

				if (selectSpan)
				{
					wpfTextView.Selection.Select(span, isReversed: false);
				}

				return (wpfTextView, newCaretPosition);
			}
			catch
			{
				return default;
			}
		}

		public static async Task<IWpfTextView?> OpenCodeWindowAsync(this IAsyncServiceProvider serviceProvider, Solution solution, string? filePath)
		{
			serviceProvider.ThrowOnNull();
			solution.ThrowOnNull();

			if (!ThreadHelper.CheckAccess() || !File.Exists(filePath))
				return null;

			ImmutableArray<DocumentId> documentIDs = solution.GetDocumentIdsWithFilePath(filePath);

			if (documentIDs.Length != 1)
				return null;

			DocumentId documentID = documentIDs[0];
			bool wasAlreadyOpened = solution.Workspace.GetOpenDocumentIds().Contains(documentID); 

			try
			{
				solution.Workspace.OpenDocument(documentID);
				return wasAlreadyOpened
					? await serviceProvider.GetWpfTextViewByFilePathAsync(filePath)
					: await serviceProvider.GetWpfTextViewAsync();
			}
			catch
			{
				return null;
			}
		}

		public static async Task ExpandAllRegionsContainingSpanAsync(this IAsyncServiceProvider serviceProvider, SnapshotSpan selectedSpan,
																	 IWpfTextView textView)
		{
			if (textView == null || serviceProvider == null || !ThreadHelper.CheckAccess())
				return;

			IOutliningManager? outliningManager = await serviceProvider.GetOutliningManagerAsync(textView);

			if (outliningManager == null)
				return;

			outliningManager.GetCollapsedRegions(selectedSpan, exposedRegionsOnly: false)
							.ForEach(region => outliningManager.Expand(region));
		}

		public static async Task<EnvDTE.Window?> OpenCodeFileNotInSolutionWithDTEAsync(this IAsyncServiceProvider serviceProvider, string? filePath)
		{
			serviceProvider.ThrowOnNull();

			if (!File.Exists(filePath) )
				return null;

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			DTE? dte = await serviceProvider.GetServiceAsync<DTE>();

			if (dte == null)
				return null;

			return TryFallBackNavigationWithDTE(filePath!, dte);
		}

		// Dynamic DTE parameter is used for the simultaneous support of VS 2019 and VS 2022.
		// This will use late COM binding which will bind to the correct VS SDK COM type 
		private static EnvDTE.Window? TryFallBackNavigationWithDTE(string filePath, dynamic dte)
		{
			try
			{
				//EnvDTE.Constants.vsViewKindCode, removed due to CS1752 error  Interop type 'EnvDTE.Constants' cannot be embedded
				const string vsViewKindCode = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";

				var window = dte.ItemOperations.OpenFile(filePath, vsViewKindCode);	
				window!.Visible = true;

				return window;
			}
			catch
			{
				return default;
			}
		}

		private static EnvDTE.TextDocument? GetTextDocumentFromWindow(this EnvDTE.Window window)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			const string TextDocumentPropertyName = "TextDocument";

			try
			{
				return window.Document?.Object(TextDocumentPropertyName) as EnvDTE.TextDocument;
			}
			catch
			{
				return default;
			}
		}
	}
}
