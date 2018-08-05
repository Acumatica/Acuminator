using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;



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

		public static IWpfTextView GetWpfTextView(this IServiceProvider serviceProvider)
		{
			IVsTextManager textManager = serviceProvider?.GetService<SVsTextManager, IVsTextManager>();
			
			if (textManager == null || textManager.GetActiveView(1, null, out IVsTextView textView) != VSConstants.S_OK)
				return null;

			IComponentModel componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
			IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService = componentModel?.GetService<IVsEditorAdaptersFactoryService>();
			return vsEditorAdaptersFactoryService?.GetWpfTextView(textView);
		}
	}
}
