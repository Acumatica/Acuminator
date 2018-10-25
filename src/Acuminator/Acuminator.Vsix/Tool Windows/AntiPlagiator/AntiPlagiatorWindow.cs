using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;


namespace Acuminator.Vsix.ToolWindows.AntiPlagiator
{
	/// <summary>
	/// The tool window for the <see cref="AntiPlagiatorWindow"/>.
	/// </summary>
	[Guid(AntiPlagiatorWindowGuidString)]
	public class AntiPlagiatorWindow : ToolWindowPane
	{
		public const string AntiPlagiatorWindowGuidString = "CF9278BB-4C9E-4099-8E50-A0505B21000E";

		private readonly AntiPlagiatorWindowControl _control;

		/// <summary>
		/// Initializes a new instance of the <see cref="AntiPlagiatorWindow"/> class.
		/// </summary>
		public AntiPlagiatorWindow() : base(null)
		{
			this.Caption = VSIXResource.AntiPlagiatorWindowTitle;

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
			// the object returned by the Content property.
			_control = new AntiPlagiatorWindowControl();
			this.Content = _control;
		}

		protected override void OnClose()
		{
			if (_control?.DataContext is AntiPlagiatorWindowViewModel antiPlagiatorVM)
			{
				antiPlagiatorVM.Dispose();
			}

			base.OnClose();
		}
	}
}
