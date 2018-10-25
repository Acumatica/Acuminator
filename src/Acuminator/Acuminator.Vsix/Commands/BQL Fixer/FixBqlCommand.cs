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
using Acuminator.Vsix;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;


namespace Acuminator.Vsix.BqlFixer
{
	internal sealed class FixBqlCommand : VSCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Fix BQL Command ID.
		/// </summary>
		public const int FixBqlCommandId = 0x0103;

		private FixBqlCommand(Package aPackage) : base(aPackage, FixBqlCommandId)
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
		public static void Initialize(Package package)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new FixBqlCommand(package);
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


			// todo: check
			if (!textView.TextBuffer.EditInProgress)
			{
				var formattedDocument = document.WithSyntaxRoot(fixedRoot);
				ApplyChanges(document, formattedDocument);
			}
		}

		private void ApplyChanges(Microsoft.CodeAnalysis.Document oldDocument, Microsoft.CodeAnalysis.Document newDocument)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			newDocument.ThrowOnNull(nameof(newDocument));

			Workspace workspace = oldDocument.Project?.Solution?.Workspace;
			Microsoft.CodeAnalysis.Solution newSolution = newDocument.Project?.Solution;

			if (workspace != null && newSolution != null)
			{
				workspace.TryApplyChanges(newSolution);
			}
		}
	}
}
