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
		/// Deploy banned API and White List files.
		/// </summary>
		/// <returns>
		/// True if operation succeeds, false if it fails.
		/// </returns>
		public (bool DeployedBannedApis, bool DeployedWhiteList) DeployBannedApiFiles()
		{
			string bannedApiFile = Path.Combine(_bannedApiFolder, ApiConstants.Storage.BannedApiFile);
			string whiteListFile = Path.Combine(_bannedApiFolder, ApiConstants.Storage.WhiteListFile);

			if (!ShouldUpdateBannedApis(bannedApiFile, whiteListFile))
				return (true, true);

			return DeployBannedApis();
		}

		private bool ShouldUpdateBannedApis(string bannedApiFile, string whiteListFile) =>
			_myDocumentsStorage.ShouldUpdateStorage || !Directory.Exists(_bannedApiFolder) ||
			!File.Exists(bannedApiFile) || !File.Exists(whiteListFile);

		private (bool DeployedBannedApis, bool DeployedWhiteList) DeployBannedApis()
		{
			if (!StorageUtils.ReCreateDirectory(_bannedApiFolder))
				return (false, false);

			Assembly utilsAssembly = typeof(ApiConstants).Assembly;

			bool deployedBannedApis = DeployFileFromResource(utilsAssembly, ApiConstants.Storage.BannedApiAssemblyResourceName);
			bool deployedWhiteList  = DeployFileFromResource(utilsAssembly, ApiConstants.Storage.WhiteListAssemblyResourceName);

			return (deployedBannedApis, deployedWhiteList);
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
			string relativeFilePath = Path.ChangeExtension(relativeFilePathWithoutExtension, Constants.CodeSnippets.FileExtension);

			return Path.Combine(_bannedApiFolder, relativeFilePath);
		}
	}
}
