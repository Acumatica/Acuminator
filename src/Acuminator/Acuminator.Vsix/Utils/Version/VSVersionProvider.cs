#nullable enable

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ProjectSystem.VS.Interop;

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

            var vsAppId = await serviceProvider.GetServiceAsync<SVsAppId, IVsAppId>();

            if (vsAppId != null &&
                vsAppId.GetProperty((int)VSAPropID.VSAPROPID_ProductDisplayVersion, out object? versionObj) == VSConstants.S_OK && versionObj != null)
            {
                var vsAppIdVersion = new Version(versionObj.ToString());
                return new VSVersion(vsAppIdVersion);
            }

            Version? fileVersion = GetFileVersion();

            if (fileVersion != null)
                return new VSVersion(fileVersion);
			
            var dte = await serviceProvider.GetServiceAsync<EnvDTE.DTE>();

            if (!string.IsNullOrWhiteSpace(dte?.Version))
			{
                Version dteVersion = new Version(dte?.Version);
                return new VSVersion(dteVersion);
			}

            var unknownVersion = new Version(VSVersion.UnknownVersion, VSVersion.UnknownVersion);
            return new VSVersion(unknownVersion);
        }


        private static Version? GetFileVersion()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devenv.exe");

            if (!File.Exists(path))
                return null; // Not running inside Visual Studio

            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                return fileVersionInfo != null
                    ? new Version(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart)
                    : null;
            }
            catch (IOException)
			{
                return null;
			}
        }
    }
}
