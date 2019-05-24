using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using DTE = EnvDTE.DTE;



namespace Acuminator.Vsix.Utilities.Navigation
{
	public static class VSDocumentNavigation
	{
		public static (IWpfTextView WpfTextView, CaretPosition CaretPosition) NavigateToSymbol(this IServiceProvider serviceProvider,
																							   ISymbol symbol, bool selectSpan = true,
																							   CancellationToken cToken = default)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));
			symbol.ThrowOnNull(nameof(symbol));

			var syntaxReferences = symbol.DeclaringSyntaxReferences;

			if (syntaxReferences.Length != 1)
				return default;

			var filePath = syntaxReferences[0].SyntaxTree?.FilePath;
			var workspace = AcuminatorVSPackage.Instance.GetVSWorkspace();
			TextSpan textSpanToNavigate = GetTextSpanToNavigateFromSymbol(symbol, syntaxReferences[0], cToken);
			return AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPosition(workspace?.CurrentSolution, filePath,
																				  textSpanToNavigate, selectSpan);
		}

		private static TextSpan GetTextSpanToNavigateFromSymbol(ISymbol symbol, SyntaxReference syntaxReference, 
																CancellationToken cToken = default)
		{
			switch (symbol)
			{
				case ITypeSymbol typeSymbol:
				case IMethodSymbol methodSymbol:
				case IPropertySymbol propertySymbol:
				case IEventSymbol eventSymbol:
					var typeMemberNode = syntaxReference.GetSyntax(cToken) as MemberDeclarationSyntax;
					return typeMemberNode?.GetIdentifiers()
										  .FirstOrNullable()
										 ?.Span ?? syntaxReference.Span;
				default:
					return syntaxReference.Span;
			}
		}

		public static (IWpfTextView WpfTextView, CaretPosition CaretPosition) OpenCodeFileAndNavigateByLineAndChar(
																									  this IServiceProvider serviceProvider,
																									  Solution solution, string filePath,
																									  int lineNumber, int character)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (lineNumber < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}
			else if (character < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(character));
			}

			IWpfTextView wpfTextView = OpenCodeWindow(serviceProvider, solution, filePath);

			if (wpfTextView == null)
			{
				var (window, textDocument) = OpenCodeFileNotInSolutionWithDTE(serviceProvider, filePath);

				if (window == null)
					return default;

				wpfTextView = serviceProvider.GetWpfTextViewByFilePath(filePath);
			}

			if (wpfTextView == null)
				return default;

			try
			{
				ITextSnapshotLine textLine = wpfTextView.TextSnapshot.GetLineFromLineNumber(lineNumber);
				int absoluteOffset = textLine.Start + character;
				SnapshotPoint point = wpfTextView.TextSnapshot.GetPoint(absoluteOffset);
				SnapshotSpan span = new SnapshotSpan(point, length: 0);

				serviceProvider.ExpandAllRegionsContainingSpan(span, wpfTextView);
				CaretPosition newCaretPosition = wpfTextView.MoveCaretTo(absoluteOffset);

				wpfTextView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
				return (wpfTextView, newCaretPosition);
			}
			catch
			{
				return default;
			}
		}

		public static (IWpfTextView WpfTextView, CaretPosition CaretPosition) OpenCodeFileAndNavigateToPosition(
																					this IServiceProvider serviceProvider,
																					Solution solution, string filePath, 
																					TextSpan? spanToNavigate = null,
																					bool selectSpan = true)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			IWpfTextView wpfTextView = OpenCodeWindow(serviceProvider, solution, filePath);

			if (wpfTextView == null)
			{
				var (window, textDocument) = OpenCodeFileNotInSolutionWithDTE(serviceProvider, filePath);

				if (window == null)
					return default;

				wpfTextView = serviceProvider.GetWpfTextViewByFilePath(filePath);
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
				serviceProvider.ExpandAllRegionsContainingSpan(span, wpfTextView);
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

		public static IWpfTextView OpenCodeWindow(this IServiceProvider serviceProvider, Solution solution, string filePath)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));
			solution.ThrowOnNull(nameof(solution));

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
					? serviceProvider.GetWpfTextViewByFilePath(filePath)
					: serviceProvider.GetWpfTextView(); 			
			}
			catch
			{
				return null;
			}
		}

		public static void ExpandAllRegionsContainingSpan(this IServiceProvider serviceProvider, SnapshotSpan selectedSpan, IWpfTextView textView)
		{
			if (textView == null)
				return;

			IOutliningManager outliningManager = serviceProvider?.GetOutliningManager(textView);

			if (outliningManager == null)
				return;

			outliningManager.GetCollapsedRegions(selectedSpan, exposedRegionsOnly: false)
							.ForEach(region => outliningManager.Expand(region));
		}

#pragma warning disable VSTHRD010
		public static (EnvDTE.Window Window, EnvDTE.TextDocument TextDocument) OpenCodeFileNotInSolutionWithDTE(
																									this IServiceProvider serviceProvider, 
																									string filePath)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (!ThreadHelper.CheckAccess() || !File.Exists(filePath) || !(serviceProvider.GetService(typeof(DTE)) is DTE dte))
				return default;

			try
			{
				//EnvDTE.Constants.vsViewKindCode, removed due to CS1752 error  Interop type 'EnvDTE.Constants' cannot be embedded
				const string vsViewKindCode = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";  
				var window = dte.ItemOperations.OpenFile(filePath, vsViewKindCode);
				var textDocument = window?.GetTextDocumentFromWindow();

				if (textDocument == null)
					return default;

				window.Visible = true;
				textDocument.StartPoint.TryToShow();
				return (window, textDocument);
			}
			catch
			{
				return default;
			}
		}

		private static EnvDTE.TextDocument GetTextDocumentFromWindow(this EnvDTE.Window window)
		{
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
#pragma warning restore VSTHRD010
	}
}
