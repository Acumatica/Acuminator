#nullable enable

using System;
using System.ComponentModel.Design;

using Microsoft.VisualStudio.Shell;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace Acuminator.Vsix
{
	/// <summary>
	/// Base command handler
	/// </summary>
	internal abstract class VSCommandBase
	{
		protected const int NOT_INITIALIZED = 0, INITIALIZED = 1;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		protected Guid DefaultCommandSet { get; } = new Guid(AcuminatorVSPackage.AcuminatorDefaultCommandSetGuidString);

		/// <summary>
		/// VS AsyncPackage that provides this command, not null.
		/// </summary>
		protected AsyncPackage Package { get; }

		/// <summary>
		/// Initializes a new instance of the command.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		protected VSCommandBase(AsyncPackage package, OleMenuCommandService commandService, int commandID, Guid? customCommandSet = null)
		{
			commandService.ThrowOnNull();

			Package = package.CheckIfNull();
			Guid commandSet = customCommandSet ?? DefaultCommandSet;

			var menuCommandID = new CommandID(commandSet, commandID);
			var menuItem = new OleMenuCommand(CommandCallback, menuCommandID)
			{
				// This defers the visibility logic back to the VisibilityConstraints in the .vsct file
				Supported = false
			};

			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		protected IAsyncServiceProvider ServiceProvider => Package;
		
		protected abstract void CommandCallback(object sender, EventArgs e);	
	}
}
