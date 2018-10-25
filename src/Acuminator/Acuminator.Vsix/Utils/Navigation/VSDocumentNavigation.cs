using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using DTE = EnvDTE.DTE;



namespace Acuminator.Vsix.Utilities.Navigation
{
	public static class VSDocumentNavigation
	{
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
				return default;

			try
			{
				ITextSnapshotLine textLine = wpfTextView.TextSnapshot.GetLineFromLineNumber(lineNumber);
				int absoluteOffset = textLine.Start + character;
				SnapshotPoint point = wpfTextView.TextSnapshot.GetPoint(absoluteOffset);
				SnapshotSpan span = new SnapshotSpan(point, length: 0);

				serviceProvider.ExpandAllRegionsContainingSpan(span, wpfTextView);
				CaretPosition newCaretPosition = wpfTextView.MoveCaretTo(absoluteOffset);

				if (!wpfTextView.TextViewLines.ContainsBufferPosition(newCaretPosition.BufferPosition))
				{
					wpfTextView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
				}

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
																					int? caretPosition = null)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (caretPosition.HasValue && caretPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(caretPosition));
			}

			IWpfTextView wpfTextView = OpenCodeWindow(serviceProvider, solution, filePath);

			if (wpfTextView == null)
				return default;

			try
			{
				if (caretPosition == null)
				{
					return (wpfTextView, wpfTextView.Caret.Position);
				}

				SnapshotPoint point = wpfTextView.TextSnapshot.GetPoint(caretPosition.Value);
				SnapshotSpan span = new SnapshotSpan(point, length: 0);
				serviceProvider.ExpandAllRegionsContainingSpan(span, wpfTextView);
				CaretPosition newCaretPosition = wpfTextView.MoveCaretTo(caretPosition.Value);

				if (!wpfTextView.TextViewLines.ContainsBufferPosition(newCaretPosition.BufferPosition))
				{
					wpfTextView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
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

			DocumentId documentId = documentIDs[0];
			bool wasAlreadyOpened = solution.IsFileOpen(documentId);

			try
			{
				solution.Workspace.OpenDocument(documentId);
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

		private static bool IsFileOpen(this Solution solution, DocumentId documentID) => 
			solution.Workspace.GetOpenDocumentIds()
							  .Contains(documentID);		
	}
}
