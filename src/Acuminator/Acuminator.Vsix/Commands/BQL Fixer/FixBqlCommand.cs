using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using Acuminator.Vsix.Formatter;

namespace Acuminator.Vsix.BqlFixer
{
	internal sealed class FixBqlCommand : VSCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Fix BQL Command ID.
		/// </summary>
		public const int FixBqlCommandId = 0x0103;

		protected override bool CanModifyDocument => true;

		private FixBqlCommand(AsyncPackage package, OleMenuCommandService commandService) :
						 base(package, commandService, FixBqlCommandId)
		{
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static FixBqlCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">The OLE command service.</param>
		public static void Initialize(AsyncPackage package, OleMenuCommandService commandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new FixBqlCommand(package, commandService);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e) =>
			CommandCallbackAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(FixBqlCommand)}");

		private async System.Threading.Tasks.Task CommandCallbackAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
			IWpfTextView textView = await ServiceProvider.GetWpfTextViewAsync();

			if (textView == null)
				return;

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			ITextSnapshotLine caretLine = caretPosition.GetContainingLine();

			if (caretLine == null)
				return;

			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null)
				return;

			await TaskScheduler.Default;    //Go to background thread

			SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync();
			SemanticModel semanticModel = await document.GetSemanticModelAsync();

			if (syntaxRoot == null || semanticModel == null)
				return;

			//TextSpan lineSpan = TextSpan.FromBounds(caretLine.Start.Position, caretLine.End.Position);
			//var memberNode = syntaxRoot.FindNode(lineSpan) as MemberDeclarationSyntax;

			//         if (memberNode == null)
			//             return;

			SyntaxNode fixedRoot;
			#region todo
			if (textView.Selection.IsActive && !textView.Selection.IsEmpty) // if has selection
			{
				// todo: check selection

				//// Find all nodes within the span and format them
				//var selectionSpan = TextSpan.FromBounds(textView.Selection.Start.Position, textView.Selection.End.Position);
				//SyntaxNode topNode = syntaxRoot.FindNode(selectionSpan); // can, return top node that intersects with selectionSpan, so we need SpanWalker here

				//if (topNode == null)
				//	return; // nothing to format (e.g. selection contains only trivia)

				//var spanWalker = new SpanWalker(selectionSpan);
				//spanWalker.Visit(topNode);

				//if (spanWalker.NodesWithinSpan.Count == 0)
				//	return;

				//fixedRoot = syntaxRoot.ReplaceNodes(spanWalker.NodesWithinSpan, (o, r) => formatter.Format(o, semanticModel));
			}
			else
			{
			}
			#endregion
			fixedRoot = new AngleBracesBqlRewriter(semanticModel).Visit(syntaxRoot);
			var newDocument = document.WithSyntaxRoot(fixedRoot);
			SyntaxNode newSyntaxRoot = await newDocument.GetSyntaxRootAsync();
			SemanticModel newSemanticModel = await newDocument.GetSemanticModelAsync();

			if (newSyntaxRoot == null || newSemanticModel == null)
				return;

			// have to format, because cannot save all original indention
			BqlFormatter formatter = BqlFormatter.FromTextView(textView);
			var formatedRoot = formatter.Format(newSyntaxRoot, newSemanticModel);

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken); // Return to UI thread

			if (!textView.TextBuffer.EditInProgress)
			{
				var formattedDocument = document.WithSyntaxRoot(formatedRoot);
				ApplyChanges(document, formattedDocument);
			}
		}

		private void ApplyChanges(Document oldDocument, Document newDocument)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			newDocument.ThrowOnNull(nameof(newDocument));

			Workspace workspace = oldDocument.Project?.Solution?.Workspace;
			Solution newSolution = newDocument.Project?.Solution;

			if (workspace != null && newSolution != null)
			{
				workspace.TryApplyChanges(newSolution);
			}
		}
	}
}
