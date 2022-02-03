using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
	/// <summary>
	/// An out of process VS setting provider.
	/// </summary>
	internal static class OutOfProcessSettingValueProvider
	{
		internal static bool IsOutOfProcessEnabled(this AcuminatorVSPackage package, Workspace workspace)
		{
			package.ThrowOnNull(nameof(package));
			package.VSVersion.ThrowOnNull($"{nameof(AcuminatorVSPackage)}.{nameof(AcuminatorVSPackage.VSVersion)}");

			if (!package.VSVersion.VS2019OrNewer)
				return false;

			// Faster version gets setting OOP64Bit from the VS session store. If it is true then the OOP is enabled
			bool? outOfProcessFromSettingsStore = GetOutOfProcessSettingFromSessionStore(package);

			if (outOfProcessFromSettingsStore == true)
				return true;

			// If OOP is false or its retrieval failed then we need to resort to the internal Roslyn helper RemoteHostOptions.IsUsingServiceHubOutOfProcess
			if (workspace?.Services != null)
			{
				Type remoteHostOptionsType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
											  where assembly.GetName().Name == "Microsoft.CodeAnalysis.Remote.Workspaces"
											  from type in assembly.GetTypes()
											  where type.IsClass && type.IsAbstract && type.IsSealed && !type.IsPublic && type.Name == "RemoteHostOptions"
											  select type)
											.SingleOrDefault();
				MethodInfo isUsingServiceHubOutOfProcess = remoteHostOptionsType?.GetMethod("IsUsingServiceHubOutOfProcess",
																							BindingFlags.Static | BindingFlags.Public);

				object isOutOfProcessFromRoslynInternalsObj = isUsingServiceHubOutOfProcess?.Invoke(null, new object[] { workspace.Services });

				if (isOutOfProcessFromRoslynInternalsObj is bool isOutOfProcessFromRoslynInternals)
					return isOutOfProcessFromRoslynInternals;
			}

			return false;
		}

		private static bool? GetOutOfProcessSettingFromSessionStore(AcuminatorVSPackage package)
		{
			const bool defaultOutOfProcessValue = true;
			const string settingsStoreOutOfProcessValuePath = @"Roslyn\Internal\OnOff\Features";
			const string OutOfProcessPropertyName = "OOP64Bit";

			var shellSettingsManager = new ShellSettingsManager(package);
			SettingsStore settingsStore = shellSettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

			if (settingsStore == null)
				return null;
			else if (!settingsStore.CollectionExists(settingsStoreOutOfProcessValuePath))
				return defaultOutOfProcessValue;

			var propertyNames = settingsStore.GetPropertyNames(settingsStoreOutOfProcessValuePath);

			if (!propertyNames.Contains(OutOfProcessPropertyName))
				return defaultOutOfProcessValue;

			int? outOfProcessValue = settingsStore.GetInt32(settingsStoreOutOfProcessValuePath, OutOfProcessPropertyName) as int?;

			return outOfProcessValue.HasValue
				? outOfProcessValue == 1
				: defaultOutOfProcessValue;
		}
	}
}
