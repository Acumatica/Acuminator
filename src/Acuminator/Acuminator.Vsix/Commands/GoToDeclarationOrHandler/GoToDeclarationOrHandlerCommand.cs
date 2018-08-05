using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Acuminator.Utilities;
using Acuminator.Vsix;
using Acuminator.Vsix.Utilities;


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
			//IWpfTextView textView = ServiceProvider.GetWpfTextView();

			//if (textView == null)
			//	return;

			//BqlFormatter formatter = CreateFormatter(textView);

			//SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			//Microsoft.CodeAnalysis.Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

			//SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			//SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			//SyntaxNode formattedRoot;
			
			//if (textView.Selection.IsActive && !textView.Selection.IsEmpty) // if has selection
			//{
			//	// Find all nodes within the span and format them
			//	var selectionSpan = TextSpan.FromBounds(textView.Selection.Start.Position, textView.Selection.End.Position);
			//	SyntaxNode topNode = syntaxRoot.FindNode(selectionSpan); // can, return top node that intersects with selectionSpan, so we need SpanWalker here

			//	if (topNode == null)
			//		return; // nothing to format (e.g. selection contains only trivia)
				
			//	var spanWalker = new SpanWalker(selectionSpan);
			//	spanWalker.Visit(topNode);

			//	if (spanWalker.NodesWithinSpan.Count == 0)
			//		return;

			//	formattedRoot = syntaxRoot.ReplaceNodes(spanWalker.NodesWithinSpan, (o, r) => formatter.Format(o, semanticModel));
			//}
			//else
			//{
			//	formattedRoot = formatter.Format(syntaxRoot, semanticModel);
			//}
			
			//if (!textView.TextBuffer.EditInProgress)
			//{
			//	var formattedDocument = document.WithSyntaxRoot(formattedRoot);
			//	ApplyChanges(document, formattedDocument);
			//}
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
