using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PX.Analyzers.Vsix.Formatter
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class FormatBqlCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("3cd59430-1e8d-40af-b48d-9007624b3d77");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly Package package;

		/// <summary>
		/// Initializes a new instance of the <see cref="FormatBqlCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private FormatBqlCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException("package");
			}

			this.package = package;

			OleMenuCommandService commandService = GetService<IMenuCommandService, OleMenuCommandService>();
			if (commandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				//var menuItem = new MenuCommand(this.FormatButtonCallback, menuCommandID);
				var menuItem = new OleMenuCommand(this.FormatButtonCallback, menuCommandID);
				menuItem.BeforeQueryStatus += QueryFormatButtonStatus;
				commandService.AddCommand(menuItem);
			}
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
		/// Gets the service provider from the owner package.
		/// </summary>
		private IServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			Instance = new FormatBqlCommand(package);
		}

		private void QueryFormatButtonStatus(object sender, EventArgs e)
		{
			var menuCommand = sender as OleMenuCommand;
			var dte = GetService<DTE, DTE2>();
			if (menuCommand != null)
			{
				bool visible = false;
				bool enabled = false;

				if (dte.ActiveDocument != null)
				{
					string fileExtension = System.IO.Path.GetExtension(dte.ActiveDocument.FullName);
					visible = !String.IsNullOrEmpty(fileExtension) && fileExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
					enabled = !dte.ActiveDocument.ReadOnly;
				}

				menuCommand.Visible = visible;
				menuCommand.Enabled = enabled;
			}
		}

		private void FormatButtonCallback(object sender, EventArgs e)
		{
			IWpfTextView textView = GetTextView();
			BqlFormatter formatter = CreateFormatter(textView);

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			Microsoft.CodeAnalysis.Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedRoot;
			
			if (textView.Selection.IsActive && !textView.Selection.IsEmpty) // if has selection
			{
				// Find all nodes within the span and format them
				var selectionSpan = Microsoft.CodeAnalysis.Text.TextSpan
					.FromBounds(textView.Selection.Start.Position, textView.Selection.End.Position);

				SyntaxNode topNode = syntaxRoot.FindNode(selectionSpan); // can, return top node that intersects with selectionSpan, so we need SpanWalker here
				if (topNode == null) return; // nothing to format (e.g. selection contains only trivia)
				
				var spanWalker = new SpanWalker(selectionSpan);
				spanWalker.Visit(topNode);
				if (spanWalker.NodesWithinSpan.Count == 0) return;

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
			if (oldDocument == null) throw new ArgumentNullException(nameof (oldDocument));
			if (newDocument == null) throw new ArgumentNullException(nameof (newDocument));

			var workspace = oldDocument.Project?.Solution?.Workspace;
			var newSolution = newDocument.Project?.Solution;
			if (workspace != null && newSolution != null)
			{
				workspace.TryApplyChanges(newSolution);
			}
		}

		private BqlFormatter CreateFormatter(IWpfTextView textView)
		{
			if (textView == null) throw new ArgumentNullException(nameof (textView));

			int indentSize = textView.Options.GetOptionValue(DefaultOptions.IndentSizeOptionId);
			int tabSize = textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
			bool convertTabsToSpaces = textView.Options.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
			string newLineCharacter = textView.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);

			return new BqlFormatter(newLineCharacter, !convertTabsToSpaces, tabSize, indentSize);
		}

		private IWpfTextView GetTextView()
		{
			var textManager = GetService<SVsTextManager, IVsTextManager>();
			textManager.GetActiveView(1, null, out IVsTextView textView);

			return GetEditorAdaptersFactoryService().GetWpfTextView(textView);
		}

		private IVsEditorAdaptersFactoryService GetEditorAdaptersFactoryService()
		{
			IComponentModel componentModel = GetService<SComponentModel, IComponentModel>();
			return componentModel.GetService<IVsEditorAdaptersFactoryService>();
		}

		private T GetService<T>()
		{
			return (T) ServiceProvider.GetService(typeof (T));
		}

		private TActual GetService<TRequested, TActual>()
		{
			return (TActual) ServiceProvider.GetService(typeof (TRequested));
		}
	}
}
