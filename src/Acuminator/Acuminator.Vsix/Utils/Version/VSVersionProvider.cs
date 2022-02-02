#nullable enable

using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using Community.VisualStudio.Toolkit;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// The Visual Studio version provider.
	/// </summary>
	internal static class VSVersionProvider
	{
		public static async Task<VSVersion> GetVersionAsync(IAsyncServiceProvider serviceProvider)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			Version? shellVersion = await VS.Shell.GetVsVersionAsync();

			if (shellVersion != null)
				return new VSVersion(shellVersion);

			var unknownVersion = new Version(VSVersion.UnknownVersion, VSVersion.UnknownVersion);
			return new VSVersion(unknownVersion);
		}
	}
}