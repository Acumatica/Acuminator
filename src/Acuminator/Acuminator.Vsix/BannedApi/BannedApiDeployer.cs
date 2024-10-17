#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Acuminator.Utilities.BannedApi;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Storage;

namespace Acuminator.Vsix.BannedApi
{
	/// <summary>
	/// Banned API deployment logic
	/// </summary>
	public sealed class BannedApiDeployer
	{
		private readonly AcuminatorMyDocumentsStorage _myDocumentsStorage;
		private readonly string _bannedApiFolder;

		private BannedApiDeployer(AcuminatorMyDocumentsStorage myDocumentsStorage, string bannedApiFolder)
		{
			_myDocumentsStorage = myDocumentsStorage.CheckIfNull();
			_bannedApiFolder	= bannedApiFolder.CheckIfNullOrWhiteSpace();
		}

		internal static BannedApiDeployer? Create(AcuminatorMyDocumentsStorage? myDocumentsStorage)
		{
			if (myDocumentsStorage == null)
				return null;
			try
			{
				string bannedApiFolder = Path.Combine(myDocumentsStorage.AcuminatorFolder, Constants.BannedApi.BannnedApiFolder);
				return new BannedApiDeployer(myDocumentsStorage, bannedApiFolder);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}
		}

		/// <summary>
		/// Deploy banned and allowed API files.
		/// </summary>
		/// <returns>
		/// A pair of files - Banned API and Allowed API file paths. Each file path is null if the file was not deployed.
		/// </returns>
		public (string? DeployedBannedApisFile, string? DeployedAllowedApisFile) DeployBannedApiFiles()
		{
			string bannedApiFile  = Path.Combine(_bannedApiFolder, ApiConstants.Storage.BannedApiFile);
			string allowedApiFile = Path.Combine(_bannedApiFolder, ApiConstants.Storage.AllowedApiFile);

			if (!ShouldUpdateBannedApis(bannedApiFile, allowedApiFile))
				return (null, null);

			var (deployedBannedApis, deployedAllowedApis) = DeployBannedApis();

			string? resultBannedApiFile  = deployedBannedApis ? bannedApiFile : null;
			string? resultAllowedApiFile = deployedAllowedApis ? allowedApiFile : null;

			return (resultBannedApiFile, resultAllowedApiFile);
		}

		private bool ShouldUpdateBannedApis(string bannedApiFile, string allowedApiFile) =>
			_myDocumentsStorage.ShouldUpdateStorage || !Directory.Exists(_bannedApiFolder) ||
			!File.Exists(bannedApiFile) || !File.Exists(allowedApiFile);

		private (bool DeployedBannedApis, bool DeployedAllowedApis) DeployBannedApis()
		{
			if (!StorageUtils.ReCreateDirectory(_bannedApiFolder))
				return (false, false);

			Assembly utilsAssembly = typeof(ApiConstants).Assembly;

			bool deployedBannedApis  = DeployFileFromResource(utilsAssembly, ApiConstants.Storage.BannedApiAssemblyResourceName);
			bool deployedAllowedApis = DeployFileFromResource(utilsAssembly, ApiConstants.Storage.AllowedApiAssemblyResourceName);

			return (deployedBannedApis, deployedAllowedApis);
		}

		private bool DeployFileFromResource(Assembly currentAssembly, string resourceName)
		{
			try
			{
				using (Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName))
				{
					if (resourceStream == null)
						return false;

					string filePath = TransformAssemblyResourceNameToFilePath(resourceName);

					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						resourceStream.CopyTo(fileStream);
						return true;
					}
				}
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}

		private string TransformAssemblyResourceNameToFilePath(string resourceName)
		{
			const string prefix = "Acuminator.Utilities.BannedApi.Data.";

			var sb = new StringBuilder(resourceName);
			sb = sb.Remove(resourceName.Length - ApiConstants.Storage.FileExtension.Length, ApiConstants.Storage.FileExtension.Length)
				   .Remove(startIndex: 0, length: prefix.Length)
				   .Replace('_', ' ')
				   .Replace('.', Path.DirectorySeparatorChar);

			string relativeFilePathWithoutExtension = sb.ToString();
			string relativeFilePath = Path.ChangeExtension(relativeFilePathWithoutExtension, ApiConstants.Storage.FileExtension);

			return Path.Combine(_bannedApiFolder, relativeFilePath);
		}
	}
}
