#nullable enable

using System;
using System.IO;
using System.Diagnostics;
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
				var vsAppIdVersion = new Version(ParseVersion(versionObj?.ToString() ?? ""));
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

		/// <summary>
		/// For Preview builds Microsoft returns something like "16.11.0 Preview 3.0".
		/// The <see cref="Version(string)"/> constructor gives a string format error because it doesn't expect something like this.
		/// Looking at their source code shows that they accept "major.minor.build.revision".
		/// This method parses out the Preview version string into a string <see cref="Version(string)"/> will accept if need be.
		/// </summary>
		/// <remarks>
		/// The <see cref="Version(string)"/> will error if there are &lt; 2 segments in the passed version string, which should be noted.
		/// Although it doesn't currently seem possible that Microsoft would return a string like this.
		/// </remarks>
		/// <param name="version"></param>
		private static string ParseVersion(string version)
		{
			const string previewKeyWord = "Preview";
			int previewIndex = version.IndexOf(previewKeyWord);

			if (previewIndex < 0)
				return version;

			string baseVersion = version.Substring(0, previewIndex - 1);
			int indexOfDotInPreviewVersion = version.IndexOf('.', previewIndex);
			int previewVersionStart = previewIndex + previewKeyWord.Length + 1;
			/*
			 * The Version constructor will error if there are > 4 segments.
			 */
			string previewVersion = indexOfDotInPreviewVersion < 0
			  ? version.Substring(previewVersionStart)
			  : version.Substring(previewVersionStart, indexOfDotInPreviewVersion - previewVersionStart);

			return $"{baseVersion}.{previewVersion}";
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