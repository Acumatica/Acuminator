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
		private const string AdditionalFilesBuildAction = "AdditionalFiles";

		public async Task<bool> SetBuildActionAsync(string roslynSuppressionFilePath, string buildActionToSet = null)
		{
			roslynSuppressionFilePath.ThrowOnNullOrWhiteSpace(nameof(roslynSuppressionFilePath));
			
			if (buildActionToSet.IsNullOrWhiteSpace())
			{
				buildActionToSet = AdditionalFilesBuildAction;
			}

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
