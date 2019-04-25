using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// This class implements the tool window exposed by this package and hosts a user control.
	/// </summary>
	/// <remarks>
	/// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, usually implemented by the package implementer.
	/// <para>
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its implementation of the IVsUIElementPane interface.
	/// </para>
	/// </remarks>
	[Guid(CodeMapWindowGuidString)]
	public class CodeMapWindow : ToolWindowPane
	{
		public const string CodeMapWindowGuidString = "b2a9bcca-0128-481f-9194-2a9087ea76e6";

		public CodeMapWindowControl CodeMapWPFControl { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeMapWindow"/> class.
		/// </summary>
		public CodeMapWindow() : base(null)
		{		
			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
			// the object returned by the Content property.
			CodeMapWPFControl = new CodeMapWindowControl();
			this.Content = CodeMapWPFControl;
		}

		/// <summary>
		/// This is called after our control has been created and sited. 
		/// This is a good place to initialize the control with data gathered from Visual Studio services.
		/// </summary>
		public override void OnToolWindowCreated()
		{
			base.OnToolWindowCreated();

			// Set the text that will appear in the title bar of the tool window. Note that because we need access to the package for localization,
			// we have to wait to do this here. If we used a constant string, we could do this in the constructor.
			this.Caption = VSIXResource.CodeMapWindowTitle;
		}

		protected override void OnClose()
		{
			if (CodeMapWPFControl?.DataContext is CodeMapWindowViewModel codeMapViewModel)
			{
				codeMapViewModel.Dispose();
			}

			base.OnClose();
		}
	}
}
