#nullable enable

using System;
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

			return new VSVersion(dte?.Version);
		}
	}
}
