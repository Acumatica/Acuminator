using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.Utils
{
	/// <summary>
	/// Acuminator VSIX package loader. Used by analyzers and refactorings to ensure that package will be loaded
	/// </summary>
	internal static class AcuminatorVsixPackageLoader
	{
		private const string VsixPackageType = "AcuminatorVSPackage";
		private const string ForceLoadPackageAsync = "ForceLoadPackageAsync";

		private static volatile bool _vsixPackageLoadWasDone;
		private static object _acuminatorVsixPackageLoaderLock = new object();

		/// <summary>
		/// Ensures that package loaded. A hack - the only known way to force the package load due to completely random default loading of packages by Visual Studio 
		/// </summary>
		public static void EnsurePackageLoaded()
		{
			if (!_vsixPackageLoadWasDone)
			{
				lock (_acuminatorVsixPackageLoaderLock)
				{
					if (!_vsixPackageLoadWasDone)
					{
						_vsixPackageLoadWasDone = true;
						SearchForVsixAndEnsureItIsLoadedPackageLoaded();
					}
				}
			}
		}

		/// <summary>
		/// Searches for the Visual Studio vsix package and if found (case when working via VSIX in Visual Studio IDE) ensures that package is loaded.
		/// Calls special method <see cref="ForceLoadPackageAsync"/> to load package provided by AcuminatorVSPackage type.
		/// </summary>
		private static void SearchForVsixAndEnsureItIsLoadedPackageLoaded()
		{
			var vsixAssembly = AppDomain.CurrentDomain.GetAssemblies()
													  .FirstOrDefault(a => a.GetName().Name == SharedConstants.PackageName);
			if (vsixAssembly == null)
				return;

			var acuminatorPackageType = vsixAssembly.GetExportedTypes().FirstOrDefault(t => t.Name == VsixPackageType);

			if (acuminatorPackageType == null)
				return;

			var dummyServiceCaller = acuminatorPackageType.GetMethod(ForceLoadPackageAsync, BindingFlags.Static | BindingFlags.Public);

			if (dummyServiceCaller == null)
				return;

			object loadTask = null;

			try
			{
				loadTask = dummyServiceCaller.Invoke(null, Array.Empty<object>());
			}
			catch
			{
				return;
			}

			if (loadTask is Task task)
			{
				const int defaultTimeoutSeconds = 20;
				task.Wait(TimeSpan.FromSeconds(defaultTimeoutSeconds));
			}
		}
	}
}
