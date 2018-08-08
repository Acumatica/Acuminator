using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using Acuminator.Utilities;



namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// The Visual Studio services extensions.
    /// </summary>
    internal static class VSServicesExtensions
	{
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		where TService : class
		{
			return serviceProvider?.GetService(typeof(TService)) as TService;
		}

		public static TActual GetService<TRequested, TActual>(this IServiceProvider serviceProvider)
		where TRequested : class
		where TActual : class
		{
			return serviceProvider?.GetService(typeof(TRequested)) as TActual;
		}

		internal static IOutliningManager GetOutliningManager(this IServiceProvider serviceProvider, ITextView textView)
		{
			if (serviceProvider == null || textView == null)
				return null;

			IComponentModel componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
			IOutliningManagerService outliningManagerService = componentModel?.GetService<IOutliningManagerService>();

			if (outliningManagerService == null)
				return null;
			
			return outliningManagerService.GetOutliningManager(textView);
		}

		internal static IWpfTextView GetWpfTextView(this IServiceProvider serviceProvider)
		{
			IVsTextManager textManager = serviceProvider?.GetService<SVsTextManager, IVsTextManager>();
			
			if (textManager == null || textManager.GetActiveView(1, null, out IVsTextView textView) != VSConstants.S_OK)
				return null;

			return serviceProvider.GetWpfTextViewFromTextView(textView);
		}

		/// <summary>
		/// Returns an IVsTextView for the given file path, if the given file is open in Visual Studio.
		/// </summary>
		/// <param name="packageServiceProvider">The package Service Provider.</param>
		/// <param name="filePath">Full Path of the file you are looking for.</param>
		/// <returns>
		/// The IVsTextView for this file, if it is open, null otherwise.
		/// </returns>
		internal static IWpfTextView GetWpfTextViewByFilePath(this IServiceProvider packageServiceProvider, string filePath)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (filePath.IsNullOrWhiteSpace())
				return null;

			DTE2 dte2 = packageServiceProvider?.GetService<SDTE, DTE2>();
			var oleServiceProvider = dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

			if (dte2 == null || oleServiceProvider == null)
				return null;

			ServiceProvider shellServiceProvider = new ServiceProvider(oleServiceProvider);

			IVsUIHierarchy uiHierarchy;
			uint itemID;
			IVsWindowFrame windowFrame;
			
			if (VsShellUtilities.IsDocumentOpen(shellServiceProvider, filePath, Guid.Empty, out uiHierarchy, out itemID, out windowFrame))
			{		
				IVsTextView textView = VsShellUtilities.GetTextView(windowFrame);   // Get the IVsTextView from the windowFrame
				return packageServiceProvider.GetWpfTextViewFromTextView(textView);
			}

			return null;
		}

		private static IWpfTextView GetWpfTextViewFromTextView(this IServiceProvider serviceProvider, IVsTextView vsTextView)
		{
			if (vsTextView == null)
				return null;

			IComponentModel componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
			IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService = componentModel?.GetService<IVsEditorAdaptersFactoryService>();
			return vsEditorAdaptersFactoryService?.GetWpfTextView(vsTextView);
		}
	}
}
