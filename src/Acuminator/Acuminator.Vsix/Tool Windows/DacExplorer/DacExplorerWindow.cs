using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;


namespace Acuminator.Vsix.ToolWindows.DacExplorer
{
	/// <summary>
	/// The tool window for the <see cref="DacExplorerWindow"/>.
	/// </summary>
	[Guid(DacExplorerWindowGuidString)]
	public class DacExplorerWindow : ToolWindowPane
	{
		public const string DacExplorerWindowGuidString = "ACBD2BB9-F7BF-4F07-BCB5-D375E18939D8";

		public DacExplorerControl Control { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DacExplorerWindow"/> class.
		/// </summary>
		public DacExplorerWindow() : base(null)
		{
			this.Caption = VSIXResource.DacExplorerWindowTitle;

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
			// the object returned by the Content property.
			Control = new DacExplorerControl();
			this.Content = Control;
			
		}
	}
}
