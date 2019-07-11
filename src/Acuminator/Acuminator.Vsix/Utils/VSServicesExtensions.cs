using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using Shell = Microsoft.VisualStudio.Shell;


namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// The Visual Studio services extensions.
    /// </summary>
    internal static class VSServicesExtensions
	{
		/// <summary>
		/// A synchronous version of <see cref="GetServiceAsync{TService}(Shell.IAsyncServiceProvider)"/> method. The async version should be used if possible. 
		/// </summary>
		/// <typeparam name="TService">Type of the service.</typeparam>
		/// <param name="serviceProvider">The package Service Provider.</param>
		/// <returns/>
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		where TService : class
		{
			return serviceProvider?.GetService(typeof(TService)) as TService;
		}

		public static async Task<TService> GetServiceAsync<TService>(this Shell.IAsyncServiceProvider serviceProvider)
		where TService : class
		{
			if (serviceProvider == null)
				return null;

			var service = await serviceProvider.GetServiceAsync(typeof(TService));
			return service as TService;
		}

		public static async Task<TActual> GetServiceAsync<TRequested, TActual>(this Shell.IAsyncServiceProvider serviceProvider)
		where TRequested : class
		where TActual : class
		{
			if (serviceProvider == null)
				return null;

			var service = await serviceProvider.GetServiceAsync(typeof(TRequested));
			return service as TActual;
		}

		internal static async Task<VisualStudioWorkspace> GetVSWorkspaceAsync(this Shell.IAsyncServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return null;

			var service = await serviceProvider.GetServiceAsync(typeof(SComponentModel));
			IComponentModel componentModel = service as IComponentModel;
			return componentModel?.GetService<VisualStudioWorkspace>();
		}

		internal static async Task<string> GetSolutionPathAsync(this Shell.IAsyncServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return null;

			VisualStudioWorkspace workspace = await serviceProvider.GetVSWorkspaceAsync();	
			return workspace?.CurrentSolution?.FilePath ?? string.Empty;
		}

		internal static async Task<IOutliningManager> GetOutliningManagerAsync(this Shell.IAsyncServiceProvider serviceProvider, ITextView textView)
		{
			if (serviceProvider == null || textView == null)
				return null;

			IComponentModel componentModel = await serviceProvider.GetServiceAsync<SComponentModel, IComponentModel>();
			IOutliningManagerService outliningManagerService = componentModel?.GetService<IOutliningManagerService>();

			if (outliningManagerService == null)
				return null;
			
			return outliningManagerService.GetOutliningManager(textView);
		}

		internal static async Task<IWpfTextView> GetWpfTextViewAsync(this Shell.IAsyncServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return null;

			IVsTextManager textManager = await serviceProvider.GetServiceAsync<SVsTextManager, IVsTextManager>();
			
			if (textManager == null || textManager.GetActiveView(1, null, out IVsTextView textView) != VSConstants.S_OK)
				return null;

			return await serviceProvider.GetWpfTextViewFromTextViewAsync(textView);
		}

		/// <summary>
		/// Returns an IVsTextView for the given file path, if the given file is open in Visual Studio.
		/// </summary>
		/// <param name="serviceProvider">The package Service Provider.</param>
		/// <param name="filePath">Full Path of the file you are looking for.</param>
		/// <returns>
		/// The IVsTextView for this file, if it is open, null otherwise.
		/// </returns>
		internal static async Task<IWpfTextView> GetWpfTextViewByFilePathAsync(this Shell.IAsyncServiceProvider serviceProvider, string filePath)
		{
			if (filePath.IsNullOrWhiteSpace() || serviceProvider == null)
				return null;

			if (!Shell.ThreadHelper.CheckAccess())
			{
				await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			DTE2 dte2 = await serviceProvider.GetServiceAsync<Shell.Interop.SDTE, DTE2>();
			var oleServiceProvider = dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

			if (dte2 == null || oleServiceProvider == null)
				return null;

			Shell.ServiceProvider shellServiceProvider = new Shell.ServiceProvider(oleServiceProvider);

			if (Shell.VsShellUtilities.IsDocumentOpen(shellServiceProvider, filePath, Guid.Empty, out var uiHierarchy, out var itemID, out var windowFrame))
			{
				IVsTextView textView = Shell.VsShellUtilities.GetTextView(windowFrame);   // Get the IVsTextView from the windowFrame
				return await serviceProvider.GetWpfTextViewFromTextViewAsync(textView);
			}

			return null;
		}

		private static async Task<IWpfTextView> GetWpfTextViewFromTextViewAsync(this Shell.IAsyncServiceProvider serviceProvider, IVsTextView vsTextView)
		{
			if (vsTextView == null)
				return null;

			IComponentModel componentModel = await serviceProvider.GetServiceAsync<SComponentModel, IComponentModel>();
			IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService = componentModel?.GetService<IVsEditorAdaptersFactoryService>();
			return vsEditorAdaptersFactoryService?.GetWpfTextView(vsTextView);
		}

		/// <summary>
		/// Get error items from "Error List" window asynchronously. In case of error returns <c>null</c>.
		/// </summary>
		/// <param name="serviceProvider">The package Service Provider.</param>
		/// <returns/>
		internal static async Task<List<IVsTaskItem>> GetErrorListAsync(this Shell.IAsyncServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return null;

			await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var errorService = await serviceProvider.GetServiceAsync<SVsErrorList, IVsTaskList>();

			if (errorService == null)
				return null;

			int result = VSConstants.S_OK;
			List<IVsTaskItem> taskItemsList = new List<IVsTaskItem>(capacity: 8);

			try
			{
				ErrorHandler.ThrowOnFailure(errorService.EnumTaskItems(out IVsEnumTaskItems errorItems));

				if (errorItems == null)
					return null;

				// Retrieve the task item text and check whether it is equal with one that supposed to be thrown.
				uint[] fetched = new uint[1];

				do
				{
					IVsTaskItem[] taskItems = new IVsTaskItem[1];
					result = errorItems.Next(1, taskItems, fetched);

					if (fetched[0] == 1 && taskItems[0] is IVsTaskItem2 taskItem)
					{
						taskItemsList.Add(taskItem);
					}

				}
				while (result == VSConstants.S_OK && fetched[0] == 1);

			}
			catch (System.Runtime.InteropServices.COMException e)
			{
				result = e.ErrorCode;
			}

			return result == VSConstants.S_OK
				? taskItemsList
				: null;
		}
	}
}
