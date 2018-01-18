using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
				var menuItem = new MenuCommand(this.FormatButtonCallback, menuCommandID);
				//var menuItem = new OleMenuCommand(this.FormatButtonCallback, menuCommandID);
				//menuItem.BeforeQueryStatus += QueryFormatButtonStatus;
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
				bool enabled = false;
				if (dte.ActiveDocument != null && !dte.ActiveDocument.ReadOnly)
				{
					string fileExtension = System.IO.Path.GetExtension(dte.ActiveDocument.FullName);
					enabled = !String.IsNullOrEmpty(fileExtension) && fileExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
				}

				menuCommand.Enabled = enabled;
			}
		}

		private void FormatButtonCallback(object sender, EventArgs e)
		{
			IWpfTextView textView = GetTextView();
			var caretPosition = textView.Caret.Position.BufferPosition;
			Microsoft.CodeAnalysis.Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

			var syntaxRoot = document.GetSyntaxRootAsync().Result;
			var semanticModel = document.GetSemanticModelAsync().Result;	
		}


		private IWpfTextView GetTextView()
		{
			var textManager = GetService<SVsTextManager, IVsTextManager>();
			textManager.GetActiveView(1, null, out IVsTextView textView);
			return GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
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
