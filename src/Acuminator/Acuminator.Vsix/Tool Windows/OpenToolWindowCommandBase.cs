using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace Acuminator.Vsix.ToolWindows
{
	/// <summary>
	/// Open Tool Window base command class.
	/// </summary>
	internal abstract class OpenToolWindowCommandBase<TWindow> : VSCommandBase
	where TWindow : ToolWindowPane
	{
		protected OpenToolWindowCommandBase(AsyncPackage package, OleMenuCommandService commandService, int commandID, Guid? customCommandSet = null) :
									   base(package, commandService, commandID, customCommandSet)
		{
		}

		/// <summary>
		/// Shows the tool window when the menu item is clicked.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		protected override void CommandCallback(object sender, EventArgs e) => 
			OpenToolWindowAsync()
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(OpenToolWindowAsync)}/{typeof(TWindow).Name}");

		protected virtual async Task<TWindow> OpenToolWindowAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = await Package.FindToolWindowAsync(typeof(TWindow), id: 0, create: true, Package.DisposalToken);

			if (window?.Frame == null)
			{
				throw new NotSupportedException("Cannot create tool window");
			}

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
			return window as TWindow;
		}
	}
}
