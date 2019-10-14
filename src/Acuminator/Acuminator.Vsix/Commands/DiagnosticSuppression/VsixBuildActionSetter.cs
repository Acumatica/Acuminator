using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A helper to set Build Action for newly added suppression file.
	/// </summary>
	public class VsixBuildActionSetter : ICustomBuildActionSetter
	{
		public bool SetBuildAction(string roslynSuppressionFilePath, string buildActionToSet)
		{
			roslynSuppressionFilePath.ThrowOnNullOrWhiteSpace(nameof(roslynSuppressionFilePath));
			buildActionToSet.ThrowOnNullOrWhiteSpace(nameof(buildActionToSet));

			try
			{
				#pragma warning disable VSTHRD104 // Offer async methods 
				// Justification: need to use sync API since consumer is code action operation which require synchronous execution
				// and located in the Utilities, so it can't use ThreadHelper.JoinableTaskFactory itself
				return ThreadHelper.JoinableTaskFactory.Run(() => SetBuildActionAsync(roslynSuppressionFilePath, buildActionToSet));
				#pragma warning restore VSTHRD104 
			}
			catch
			{
				return false;
			}
		}

		private async Task<bool> SetBuildActionAsync(string roslynSuppressionFilePath, string buildActionToSet)
		{
			var oldScheduler = TaskScheduler.Current;
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			EnvDTE.DTE dte = await AcuminatorVSPackage.Instance.GetServiceAsync<EnvDTE.DTE>();

			if (dte == null)
				return false;
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

			EnvDTE.ProjectItem suppressionFileDteItem = dte.Solution.FindProjectItem(roslynSuppressionFilePath);

			if (!TrySetBuildActionWithProjectItem(suppressionFileDteItem, buildActionToSet))
				return false;

			await oldScheduler;  //Return to the old scheduler
			return true;
		}

		#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
		private bool TrySetBuildActionWithProjectItem(EnvDTE.ProjectItem suppressionFileDteItem, string buildActionToSet)
		{
			if (suppressionFileDteItem == null)
				return false;

			try
			{
				EnvDTE.Property buildActionProperty = suppressionFileDteItem.Properties.Item("ItemType");

				if (buildActionProperty == null)
					return false;

				buildActionProperty.Value = buildActionToSet;
				suppressionFileDteItem.Save();
				suppressionFileDteItem.ContainingProject?.Save();
				return true;
			}
			catch (Exception)
			{
				return false;
			}		
		}
		#pragma warning restore VSTHRD010
	}
}
