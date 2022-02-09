#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;

using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using Community.VisualStudio.Toolkit;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

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
		public static readonly Guid CodeMapWindowGuid = new Guid(CodeMapWindowGuidString);

		public CodeMapWindowControl CodeMapWPFControl { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeMapWindow"/> class.
		/// </summary>
		public CodeMapWindow() : base(AcuminatorVSPackage.Instance)
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
		[SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "The extension point has void type")]
		public override async void OnToolWindowCreated()
		{
			base.OnToolWindowCreated();

			// Set the text that will appear in the title bar of the tool window. Note that because we need access to the package for localization,
			// we have to wait to do this here. If we used a constant string, we could do this in the constructor.
			this.Caption = VSIXResource.CodeMap_WindowTitle;

			if (CodeMapWPFControl == null)
				return;

			IAsyncServiceProvider serviceProvider = AcuminatorVSPackage.Instance ?? AsyncServiceProvider.GlobalProvider;

			if (serviceProvider == null)
				return;

			Workspace? workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (workspace == null)
				return;

			IWpfTextView? textView = await ThreadHelper.JoinableTaskFactory.RunAsync(serviceProvider.GetWpfTextViewAsync);
			Document? document = textView?.TextSnapshot?.GetOpenDocumentInCurrentContextWithChanges();

			if (CodeMapWPFControl.DataContext is CodeMapWindowViewModel codeMapViewModel)
				await codeMapViewModel.RefreshCodeMapOnWindowOpeningAsync(textView, document);
			else
				CodeMapWPFControl.DataContext = CodeMapWindowViewModel.InitCodeMap(workspace, textView, document);
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
