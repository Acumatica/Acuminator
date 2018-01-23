﻿using System;
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

			int indentSize = textView.Options.GetOptionValue(DefaultOptions.IndentSizeOptionId);
			int tabSize = textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
			bool convertTabsToSpaces = textView.Options.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
			string newLineCharacter = textView.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);

			SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
			Microsoft.CodeAnalysis.Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;

			SyntaxNode first;
			
			bool hasSelection = textView.Selection.IsActive && !textView.Selection.IsEmpty;
			bool isReversed = hasSelection && textView.Selection.IsReversed;

			if (hasSelection)
			{
				SnapshotPoint start = textView.Selection.Start.Position;
				SnapshotPoint end = textView.Selection.End.Position;

				first = syntaxRoot.FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(start, end));
			}
			else
			{
				first = syntaxRoot;
			}

			var formatter = new BqlFormatter(newLineCharacter, !convertTabsToSpaces, tabSize, indentSize);
			SyntaxNode formattedNode = formatter.Format(first, semanticModel);

			if (!textView.TextBuffer.EditInProgress)
			{
				string newText = formattedNode.ToFullString();
				Span replacementSpan = Span.FromBounds(first.FullSpan.Start, first.FullSpan.End);
				var snapshot = textView.TextBuffer.Replace(replacementSpan, formattedNode.ToFullString());
				if (hasSelection) // restore selection
				{
					textView.Selection.Select(new SnapshotSpan(snapshot, replacementSpan.Start, newText.Length), isReversed);
				}

				// Restore caret position
				SnapshotPoint newCaretPosition = caretPosition.TranslateTo(snapshot, PointTrackingMode.Positive);
				textView.Caret.MoveTo(newCaretPosition);
				// TODO: change caret position restoration to more accurate implementation
			}
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
