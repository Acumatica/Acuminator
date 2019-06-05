using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Open CodeMap Window command.
	/// </summary>
	internal sealed class OpenCodeMapWindowCommand : OpenToolWindowCommandBase<CodeMapWindow>
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0201;

		protected override bool CanModifyDocument => false;

		private OpenCodeMapWindowCommand(AsyncPackage package, OleMenuCommandService commandService) : 
									base(package, commandService, CommandId)
		{		
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static OpenCodeMapWindowCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="oleCommandService">The OLE command service, not null.</param>
		public static void Initialize(AsyncPackage package, OleMenuCommandService oleCommandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new OpenCodeMapWindowCommand(package, oleCommandService);
			}
		}

		protected override async Task<CodeMapWindow> OpenToolWindowAsync()
		{
			CodeMapWindow codeMapWindow = await base.OpenToolWindowAsync();

			if (codeMapWindow?.CodeMapWPFControl == null)
				return codeMapWindow;

			CodeMapWindowViewModel codeMapViewModel = codeMapWindow.CodeMapWPFControl.DataContext as CodeMapWindowViewModel;
			IWpfTextView textView = await ServiceProvider.GetWpfTextViewAsync();
			Document document = textView?.TextSnapshot?.GetOpenDocumentInCurrentContextWithChanges();

			if (document == null)
			{
				return codeMapWindow;
			}
			else if (CheckIfSameDocumentWasReOpened(codeMapViewModel, document))   //Return window in case of re-openning of the same doc
			{
				codeMapWindow.CodeMapWPFControl.DataContext = CodeMapWindowViewModel.InitCodeMap(textView, document);
				return codeMapWindow;
			}
			else if (codeMapViewModel != null)                  //Cancel operations in case of another doc and dispose of the old view model
			{
				codeMapViewModel.CancelCodeMapBuilding();
				codeMapViewModel.Dispose();
			}

			codeMapWindow.CodeMapWPFControl.DataContext = CodeMapWindowViewModel.InitCodeMap(textView, document);
			return codeMapWindow;
		}

		private bool CheckIfSameDocumentWasReOpened(CodeMapWindowViewModel codeMapViewModel, Document openedDocument)
		{
			if (codeMapViewModel == null)
				return false;

			return codeMapViewModel.Document.FilePath == openedDocument.FilePath;
		}
	}
}
