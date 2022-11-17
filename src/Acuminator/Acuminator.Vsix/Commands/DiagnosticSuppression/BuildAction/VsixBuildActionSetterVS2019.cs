#nullable enable

using System;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using Acuminator.Utilities;
using Acuminator.Vsix.Logger;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A helper to set Build Action for newly added suppression file in VS 2019 or older that can use VS COM API directy.
	/// </summary>
	public class VsixBuildActionSetterVS2019 : ICustomBuildActionSetter
	{
		public bool SetBuildAction(string roslynSuppressionFilePath, string buildActionToSet)
		{
			roslynSuppressionFilePath.ThrowOnNullOrWhiteSpace(nameof(roslynSuppressionFilePath));
			buildActionToSet.ThrowOnNullOrWhiteSpace(nameof(buildActionToSet));

			if (SharedVsSettings.VSVersion.VS2022OrNewer)
				return false;

			try
			{
				#pragma warning disable VSTHRD104 // Offer async methods 
				// Justification: need to use sync API since consumer is code action operation which require synchronous execution
				// and located in the Utilities, so it can't use ThreadHelper.JoinableTaskFactory itself
				return ThreadHelper.JoinableTaskFactory.Run(() => SetBuildActionAsync(roslynSuppressionFilePath, buildActionToSet));
				#pragma warning restore VSTHRD104 
			}
			catch (Exception ex)
			{
				AcuminatorLogger.LogException(ex);
				return false;
			}
		}

		private async Task<bool> SetBuildActionAsync(string roslynSuppressionFilePath, string buildActionToSet)
		{
			var oldScheduler = TaskScheduler.Current;
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
				EnvDTE.DTE? dte = await AcuminatorVSPackage.Instance.GetServiceAsync<EnvDTE.DTE>();

				if (dte == null)
					return false;
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

				EnvDTE.ProjectItem? suppressionFileDteItem = dte.Solution.FindProjectItem(roslynSuppressionFilePath);

				if (!TrySetBuildActionWithProjectItem(suppressionFileDteItem, buildActionToSet))
					return false;
			
				return true;
			}
			finally
			{
				await oldScheduler;  //Return to the old scheduler
			}
		}

		#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
		private bool TrySetBuildActionWithProjectItem(EnvDTE.ProjectItem? suppressionFileDteItem, string buildActionToSet)
		{
			if (suppressionFileDteItem == null)
				return false;

			try
			{
				EnvDTE.Property? buildActionProperty = suppressionFileDteItem.Properties.Item("ItemType");

				if (buildActionProperty == null)
					return false;

				buildActionProperty.Value = buildActionToSet;
				return true;
			}
			catch (Exception ex)
			{
				AcuminatorLogger.LogException(ex);
				return false;
			}		
		}
		#pragma warning restore VSTHRD010
	}
}
