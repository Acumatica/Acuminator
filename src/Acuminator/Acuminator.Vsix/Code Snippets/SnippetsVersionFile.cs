
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;

using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.CodeSnippets
{
	/// <summary>
	/// Snippets version file logic
	/// </summary>
	/// <remarks>
	/// Version file example:
	/// &lt;version &gt;3.0.0&lt;/version &gt;
	/// </remarks>
	internal static class SnippetsVersionFile
	{
		private const string VersionFileName = "SnippetsVersion.xml";
		private const string RootNode = "version";

		public static bool WriteVersionFile(string snippetsRootFolder, Version version)
		{
			version.ThrowOnNull(nameof(version));
			snippetsRootFolder.ThrowOnNullOrWhiteSpace(nameof(snippetsRootFolder));

			string snippetsVersionFilePath = Path.Combine(snippetsRootFolder, VersionFileName);
			var root = new XElement(RootNode, version);
			var snippetVersionFile = new XDocument(root);

			try
			{
				snippetVersionFile.Save(snippetsVersionFilePath);
				return true;
			}
			catch (Exception exception)
			{
				AcuminatorLogger.LogException(exception);
				return false;
			}
		}

		public static Version? TryGetExistingSnippetsVersion(string? snippetsRootFolder)
		{
			if (snippetsRootFolder.IsNullOrWhiteSpace())
				return null;

			string snippetsVersionFilePath = Path.Combine(snippetsRootFolder, VersionFileName);
			XDocument? snippetVersionFile = LoadVersionFile(snippetsVersionFilePath);

			if (snippetVersionFile?.Root == null || snippetVersionFile.Root.Name != RootNode || snippetVersionFile.Root.IsEmpty)
				return null;

			string versionString = snippetVersionFile.Root.Value;
			return Version.TryParse(versionString, out Version version)
				? version
				: null;
		}

		private static XDocument? LoadVersionFile(string snippetsVersionFilePath)
		{
			try
			{
				if (!File.Exists(snippetsVersionFilePath))
					return null;

				return XDocument.Load(snippetsVersionFilePath);
			}
			catch (Exception exception)
			{
				AcuminatorLogger.LogException(exception);
				return null;
			}
		}
	}
}
