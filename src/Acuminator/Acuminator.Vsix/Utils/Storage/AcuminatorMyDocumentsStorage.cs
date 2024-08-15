#nullable enable

using System;
using System.IO;

using Acuminator.Vsix.Logger;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Utilities.Storage
{
    /// <summary>
    /// Acuminator data storage in the My Documents folder.
    /// </summary>
    internal class AcuminatorMyDocumentsStorage
	{
		public string MyDocumentsFolder { get; }

		/// <summary>
		/// The full path to the Acuminator subfolder inside "My Documents".
		/// </summary>
		public string AcuminatorFolder { get; }

		public VersionFile VersionFile { get; }

		private AcuminatorMyDocumentsStorage(string myDocumentsFolder, string acuminatorFolder, VersionFile versionFile)
		{
			MyDocumentsFolder = myDocumentsFolder;
			AcuminatorFolder = acuminatorFolder;
			VersionFile = versionFile;
		}

		public static AcuminatorMyDocumentsStorage? TryInitialize(Version packageCurrentVersion)
		{
			packageCurrentVersion.ThrowOnNull();

			string? myDocumentsFolder = GetMyDocumentsFolderPath();

			if (myDocumentsFolder == null)
				return null;

			string? acuminatorFolder = InitializeAcuminatorFolder(myDocumentsFolder);

			if (acuminatorFolder == null)
				return null;

			var versionFile = new VersionFile(acuminatorFolder, packageCurrentVersion);
			UpdateVersionFile(versionFile);
			return new AcuminatorMyDocumentsStorage(myDocumentsFolder, acuminatorFolder, versionFile);
		}

		private static string? GetMyDocumentsFolderPath()
		{
			string? myDocumentsFolder;

			try
			{
				myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
				myDocumentsFolder = myDocumentsFolder?.NullIfWhiteSpace()?.Trim();
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}

			if (myDocumentsFolder == null || !Directory.Exists(myDocumentsFolder))
			{
				AcuminatorLogger.LogException(new Exception("User \"My Documents\" folder was not found on the machine"));
				return null;
			}

			return myDocumentsFolder;
		}

		private static string? InitializeAcuminatorFolder(string myDocumentsFolder)
		{
			string acuminatorFolder;

			try
			{
				acuminatorFolder = Path.Combine(myDocumentsFolder, AcuminatorVSPackage.PackageName);

				if (!Directory.Exists(acuminatorFolder))
					Directory.CreateDirectory(acuminatorFolder);

				return acuminatorFolder;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}
		}

		private static bool UpdateVersionFile(VersionFile versionFile)
		{
			var existingVersion = versionFile.TryGetExistingVersion();

			if (existingVersion != null && versionFile.Version <= existingVersion)
				return true;

			return versionFile.WriteVersionFile();
		}

		private static bool ReCreateDirectory(string directory)
		{
			try
			{
				if (Directory.Exists(directory))
					Directory.Delete(directory, recursive: true);

				Directory.CreateDirectory(directory);
				return true;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}

		private static bool CreateDirectory(string directory)
		{
			try
			{
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				return true;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}
	}
}
