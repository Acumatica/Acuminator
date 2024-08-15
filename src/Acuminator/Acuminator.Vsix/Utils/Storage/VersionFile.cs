#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;

using Acuminator.Vsix.Logger;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Utilities.Storage
{
	/// <summary>
	/// Version file logic
	/// </summary>
	/// <remarks>
	/// Version file example:
	/// &lt;version &gt;3.0.0&lt;/version &gt;
	/// </remarks>
	internal class VersionFile
	{
		private const string VersionFileName = "AcuminatorVersion.xml";
		private const string RootNode = "version";

		public string RootFolder { get; }

		public Version Version { get; }

		public VersionFile(string rootFolder, Version version)
		{
			RootFolder = rootFolder.CheckIfNullOrWhiteSpace();
			Version = version.CheckIfNull();
		}

		public virtual bool WriteVersionFile()
		{
			string versionFilePath = Path.Combine(RootFolder, VersionFileName);
			var root 			   = new XElement(RootNode, Version);
			var versionFile 	   = new XDocument(root);

			try
			{
				versionFile.Save(versionFilePath);
				return true;
			}
			catch (Exception exception)
			{
				AcuminatorLogger.LogException(exception);
				return false;
			}
		}

		public virtual Version? TryGetExistingVersion()
		{
			string versionFilePath = Path.Combine(RootFolder, VersionFileName);
			XDocument? versionFile = LoadVersionFile(versionFilePath);

			if (versionFile?.Root == null || versionFile.Root.Name != RootNode || versionFile.Root.IsEmpty)
				return null;

			string versionString = versionFile.Root.Value;
			return Version.TryParse(versionString, out Version existingVersion)
				? existingVersion
				: null;
		}

		protected XDocument? LoadVersionFile(string versionFilePath)
		{
			try
			{
				if (!File.Exists(versionFilePath))
					return null;

				return XDocument.Load(versionFilePath);
			}
			catch (Exception exception)
			{
				AcuminatorLogger.LogException(exception);
				return null;
			}
		}
	}
}
