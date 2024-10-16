﻿#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;

namespace Acuminator.Vsix.Formatter
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class FormatBqlCommand : VSCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Format Command ID.
		/// </summary>
		public const int FormatCommandId = 0x0101;

		/// <summary>
		/// Initializes a new instance of the <see cref="FormatBqlCommand"/> class. Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">The OLE command service.</param>
		private FormatBqlCommand(AsyncPackage package, OleMenuCommandService commandService) :
							base(package, commandService, FormatCommandId)
		{
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static FormatBqlCommand? Instance
		{
			get;
			private set;
		}

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">The OLE command service.</param>
		[MemberNotNull(nameof(Instance))]
		public static void Initialize(AsyncPackage package, OleMenuCommandService commandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new FormatBqlCommand(package, commandService);
			}
		}
#pragma warning restore CS8774

		protected override void CommandCallback(object sender, EventArgs e) =>
			CommandCallbackAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(FormatBqlCommand)}");

		private async System.Threading.Tasks.Task CommandCallbackAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			IWpfTextView? textView = await ServiceProvider.GetWpfTextViewAsync();

			if (textView == null || Package.DisposalToken.IsCancellationRequested)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			Document? document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (document == null)
				return;

			await TaskScheduler.Default;    //Go to background thread

			BqlFormatter formatter = BqlFormatter.FromTextView(textView);

			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync(default).ConfigureAwait(false);

			if (syntaxRoot == null || semanticModel == null)
				return;

			bool isPlatformReferenced = semanticModel.Compilation.GetTypeByMetadataName(TypeFullNames.PXGraph) != null;

			if (!isPlatformReferenced)
				return;

			SyntaxNode formattedRoot;

			if (textView.Selection.IsActive && !textView.Selection.IsEmpty) // if has selection
			{
				// Find all nodes within the span and format them
				var selectionSpan = TextSpan.FromBounds(textView.Selection.Start.Position, textView.Selection.End.Position);
				SyntaxNode topNode = syntaxRoot.FindNode(selectionSpan); // can, return top node that intersects with selectionSpan, so we need SpanWalker here

				if (topNode == null)
					return; // nothing to format (e.g. selection contains only trivia)

				var spanWalker = new SpanWalker(selectionSpan);
				spanWalker.Visit(topNode);

				if (spanWalker.NodesWithinSpan.Count == 0)
					return;

				formattedRoot = syntaxRoot.ReplaceNodes(spanWalker.NodesWithinSpan, 
														(oldNode, replacementNode) => formatter.Format(oldNode, semanticModel) ?? replacementNode);
			}
			else
			{
				formattedRoot = formatter.Format(syntaxRoot, semanticModel) ?? syntaxRoot;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); // Return to UI thread

			if (!textView.TextBuffer.EditInProgress && !syntaxRoot.Equals(formattedRoot))
			{
				var formattedDocument = document.WithSyntaxRoot(formattedRoot);
				ApplyChanges(document, formattedDocument);
			}
		}

		private void ApplyChanges(Document oldDocument, Document newDocument)
		{
			oldDocument.ThrowOnNull();
			newDocument.ThrowOnNull();

			Workspace? workspace = oldDocument.Project?.Solution?.Workspace;
			Solution? newSolution = newDocument.Project?.Solution;

			if (workspace != null && newSolution != null)
			{
				workspace.TryApplyChanges(newSolution);
			}
		}
	}
}
