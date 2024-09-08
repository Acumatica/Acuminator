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

		public Version PackageVersion => VersionFile.Version;

		/// <summary>
		/// Gets the version of Acuminator package that was stored in the storage.
		/// </summary>
		public Version? VersionStoredInStorage { get; }

		public bool ShouldUpdateStorage => 
			VersionStoredInStorage == null || PackageVersion > VersionStoredInStorage;

		private AcuminatorMyDocumentsStorage(string myDocumentsFolder, string acuminatorFolder, VersionFile versionFile,
											 Version? versionStoredInStorage)
		{
			MyDocumentsFolder 	   = myDocumentsFolder;
			AcuminatorFolder 	   = acuminatorFolder;
			VersionFile 		   = versionFile;
			VersionStoredInStorage = versionStoredInStorage;
		}

		public static AcuminatorMyDocumentsStorage? TryInitialize(string packageCurrentVersion)
		{
			try 
			{
				var packageVersion = new Version(packageCurrentVersion);
				return TryInitialize(packageVersion);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}
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
			var versionStoredInStorage = versionFile.TryGetExistingVersion();

			if (!UpdateVersionFile(versionFile, versionStoredInStorage))
				return null;

			return new AcuminatorMyDocumentsStorage(myDocumentsFolder, acuminatorFolder, versionFile, versionStoredInStorage);
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
				StorageUtils.CreateDirectory(acuminatorFolder);
				return acuminatorFolder;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}
		}

		private static bool UpdateVersionFile(VersionFile versionFile, Version? versionStoredInStorage)
		{
			if (versionStoredInStorage != null && versionFile.Version <= versionStoredInStorage)
				return true;

			return versionFile.WriteVersionFile();
		}
	}
}
