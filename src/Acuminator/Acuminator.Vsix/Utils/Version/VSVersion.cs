#nullable enable

using System;
using System.IO;
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
using Microsoft.VisualStudio.Shell;

using Microsoft.VisualStudio.ProjectSystem.VS.Interop;

using EnvDTE80;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// The Visual Studio version information.
    /// </summary>
    internal class VSVersion
	{
		public string RawVersion { get; }

		private VSVersion(string? version)
		{
			RawVersion = version.NullIfWhiteSpace() ?? string.Empty;
		}

        public static async Task<VSVersion> GetVersionAsync(IAsyncServiceProvider serviceProvider)
		{
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			var dte = await serviceProvider.GetServiceAsync<EnvDTE.DTE>();
			string? dteVersion = dte?.Version;

			var shell = await serviceProvider.GetServiceAsync<Microsoft.VisualStudio.Shell.Interop.SVsShell, Microsoft.VisualStudio.Shell.Interop.IVsShell>();

			object? version = null;
			var res = shell?.GetProperty((int)Microsoft.VisualStudio.Shell.Interop.__VSSPROPID5.VSSPROPID_ReleaseVersion, out version);
			string? shellVersion = version?.ToString();

            var fileVersion = GetFileVersion();


            var vsAppId = await serviceProvider.GetServiceAsync<SVsAppId, IVsAppId>();
            var res2 = vsAppId.GetProperty((int)VSAPropID.VSAPROPID_ProductDisplayVersion, out object? semanticVersionObj);

            string? vsAppIdVersion = semanticVersionObj?.ToString();


            return new VSVersion(dte?.Version);
		}

        public static Version GetFileVersion()
        {

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devenv.exe");

            if (File.Exists(path))
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                string verName = fvi.ProductVersion;

                for (int i = 0; i < verName.Length; i++)
                {
                    if (!char.IsDigit(verName, i) && verName[i] != '.')
                    {
                        verName = verName.Substring(0, i);
                        break;
                    }
                }
                return new Version(verName);
            }
            else
                return new Version(0, 0); // Not running inside Visual Studio!
        }
    }
}
