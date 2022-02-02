using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.VisualStudio.Threading;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// The polyfills with missing funcitonality in VS SDK 15 for VS 2017. 
	/// TODO: Delete after dropping support of VS 2017.
	/// </summary>
	internal static class PolyfillsForVS2017
	{
		/// <summary>
		/// Records error information when the given <see cref="T:System.Threading.Tasks.Task" /> faults.
		/// </summary>
		/// <remarks>
		/// A replacement to <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>'s FileAndForget method
		/// that is not available in the Microsoft.VisualStudio.SDK 15. <br/><br/>
		/// TODO: Should be replaced with <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>.FileAndForget if we drop support for VS 2017.
		/// </remarks>
		/// <param name="task">The task which fault should be filed.</param>
		/// <param name="faultEventName">
		/// Must not be null or empty. This string will be used as a name for the failure event logged on <paramref name="task" /> fault.
		/// According to Microsoft recommendations it should consist of 3 parts and follow the following pattern: <br/>
		/// [product]/[featureName]/[entityName]. <br/>
		/// FeatureName could be a one-level feature or feature hierarchy delimited by "/". Examples: <br/>
		/// vs/platform/opensolution; <br/>
		/// vs/platform/editor/lightbulb/fixerror; <br/>
		/// </param>
		/// <param name="faultDescription"> A description to include in the error message for VS Activity Log when <paramref name="task" /> faults.</param>
		/// <param name="fileOnlyIf">
		/// An optional exception filter that must return <c>true</c> for the exception to be reported to the VS activity log.
		/// </param>
		public static void FileAndForget(this Task task, string faultEventName, string faultDescription = null,
										 Func<Exception, bool> fileOnlyIf = null)
		{
			task.ThrowOnNull(nameof(task));
			faultEventName.ThrowOnNullOrEmpty(nameof(faultEventName));

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
#pragma warning disable VSTHRD110 // Observe result of async calls

			ThreadHelper.JoinableTaskFactory.RunAsync(() => ProcessTaskAndLogFailuresAsync(task, faultEventName, faultDescription, fileOnlyIf));

#pragma warning restore VSTHRD110 // Observe result of async calls
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
		}

		private static async Task ProcessTaskAndLogFailuresAsync(Task task, string faultEventName, string faultDescription, 
																 Func<Exception, bool> fileOnlyIf)
		{
			try
			{
				#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

				await task.ConfigureAwait(false);

				#pragma warning restore VSTHRD003
			}
			catch (Exception exception) when (fileOnlyIf == null || fileOnlyIf(exception))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				string errorMsg = faultDescription.IsNullOrWhiteSpace()
					? exception.ToString()
					: faultDescription + Environment.NewLine + exception.ToString();

				Microsoft.VisualStudio.Shell.ActivityLog.TryLogError(faultEventName, errorMsg);
			}
		}
	}
}
