using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Vsix;
using Acuminator.Vsix.Utilities;



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
		/// Initializes a new instance of the <see cref="FormatBqlCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="aPackage">Owner package, not null.</param>
		private FormatBqlCommand(Package aPackage) : base(aPackage, FormatCommandId)
		{
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static FormatBqlCommand Instance
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
				Instance = new FormatBqlCommand(package);
			}
		}

		protected override void CommandCallback(object sender, EventArgs e)
		{
			IWpfTextView textView = ServiceProvider.GetWpfTextView();

			if (textView == null)
				return;

			BqlFormatter formatter = CreateFormatter(textView);

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null) return;

			(SyntaxNode syntaxRoot, SemanticModel semanticModel) = ThreadHelper.JoinableTaskFactory.Run(
				async () => (await document.GetSyntaxRootAsync(), await document.GetSemanticModelAsync()));

			if (syntaxRoot == null || semanticModel == null)
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

				formattedRoot = syntaxRoot.ReplaceNodes(spanWalker.NodesWithinSpan, (o, r) => formatter.Format(o, semanticModel));
			}
			else
			{
				formattedRoot = formatter.Format(syntaxRoot, semanticModel);
			}
			
			if (!textView.TextBuffer.EditInProgress)
			{
				var formattedDocument = document.WithSyntaxRoot(formattedRoot);
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

		private BqlFormatter CreateFormatter(IWpfTextView textView)
		{
			textView.ThrowOnNull(nameof(textView));

			int indentSize = textView.Options.GetOptionValue(DefaultOptions.IndentSizeOptionId);
			int tabSize = textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
			bool convertTabsToSpaces = textView.Options.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
			string newLineCharacter = textView.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);

			return new BqlFormatter(newLineCharacter, !convertTabsToSpaces, tabSize, indentSize);
		}
	}
}
