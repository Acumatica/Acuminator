﻿#nullable enable

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;

using Microsoft.VisualStudio.Threading;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A helper to set Build Action for newly added suppression file in VS 2022 or newer that can't use VS COM API directy.
	/// </summary>
	public class VsixBuildActionSetterVS2022 : ICustomBuildActionSetter
	{
		public bool SetBuildAction(string roslynSuppressionFilePath, string buildActionToSet)
		{
			roslynSuppressionFilePath.ThrowOnNullOrWhiteSpace();
			buildActionToSet.ThrowOnNullOrWhiteSpace();

			if (!SharedVsSettings.VSVersion?.VS2022OrNewer == true)
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

			try
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				dynamic? dte = GetDTE();

				if (dte == null)
					return false;

				dynamic? suppressionFileDteItem = dte.Solution?.FindProjectItem(roslynSuppressionFilePath);

				if (!TrySetBuildActionWithProjectItem(suppressionFileDteItem, buildActionToSet))
					return false;

				return true;
			}
			finally
			{
				await oldScheduler;  //Return to the old scheduler
			}		
		}

		private bool TrySetBuildActionWithProjectItem(dynamic? suppressionFileDteItem, string buildActionToSet)
		{
			if (suppressionFileDteItem == null)
				return false;

			try
			{
				dynamic? buildActionProperty = suppressionFileDteItem.Properties?.Item("ItemType");

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

		private static dynamic? GetDTE()
		{
			Assembly? interopAssembly = GetInteropAssembly();

			if (interopAssembly == null)
				return null;

			string dteTypeName = typeof(EnvDTE.DTE).FullName;
			Type? dteType = interopAssembly.ExportedTypes.FirstOrDefault(t => t.FullName == dteTypeName);

			if (dteType == null)
				return null;

			var serviceProvider = AcuminatorVSPackage.Instance as IServiceProvider;
			return serviceProvider.GetService(dteType);
		}

		private static Assembly? GetInteropAssembly()
		{
			const string dteAssemblyVS2022 = "Microsoft.VisualStudio.Interop";
			return AppDomain.CurrentDomain.GetAssemblies()
										  .FirstOrDefault(assembly => assembly.GetName().Name == dteAssemblyVS2022);
		}
	}
}
