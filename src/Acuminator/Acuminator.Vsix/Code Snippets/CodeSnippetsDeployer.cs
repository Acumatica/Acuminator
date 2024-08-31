#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Storage;

namespace Acuminator.Vsix.CodeSnippets
{
	/// <summary>
	/// Code Snippets deployment logic
	/// </summary>
	public sealed class CodeSnippetsDeployer
	{
		private const string OldSnippetsVersionFileName = "SnippetsVersion.xml";

		private readonly AcuminatorMyDocumentsStorage _myDocumentsStorage;
		private readonly string _snippetsFolder;

		private CodeSnippetsDeployer(AcuminatorMyDocumentsStorage myDocumentsStorage, string snippetsFolder)
		{
			_myDocumentsStorage = myDocumentsStorage.CheckIfNull();
			_snippetsFolder = snippetsFolder.CheckIfNullOrWhiteSpace();
		}

		internal static CodeSnippetsDeployer? Create(AcuminatorMyDocumentsStorage? myDocumentsStorage)
		{
			if (myDocumentsStorage == null)
				return null;
			try
			{
				string snippetsFolder = Path.Combine(myDocumentsStorage.AcuminatorFolder, Constants.CodeSnippets.CodeSnippetsFolder);
				return new CodeSnippetsDeployer(myDocumentsStorage, snippetsFolder);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return null;
			}
		}

		/// <summary>
		/// Deploy code snippets.
		/// </summary>
		/// <returns>
		/// True if it succeeds, false if it fails.
		/// </returns>
		public bool DeployCodeSnippets()
		{
			if (Directory.Exists(_snippetsFolder))
			{
				TryDeleteOldSnippetsVersionFile();

				if (_myDocumentsStorage.ShouldUpdateStorage)
					return true;
			}

			return DeployCodeSnippetsImpl();
		}

		private void TryDeleteOldSnippetsVersionFile()
		{
			string snippetsFilePath = Path.Combine(_snippetsFolder, OldSnippetsVersionFileName);

			try
			{
				if (File.Exists(snippetsFilePath))
					File.Delete(snippetsFilePath);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
			}
		}

		private bool DeployCodeSnippetsImpl()
		{
			if (!StorageUtils.ReCreateDirectory(_snippetsFolder))
				return false;

			return DeployCodeSnippetsFromAssemblyResources();
		}

		private bool DeployCodeSnippetsFromAssemblyResources()
		{
			Assembly currentAssembly = Assembly.GetExecutingAssembly();
			var snippetResources = currentAssembly.GetManifestResourceNames()
												 ?.Where(rName => rName.EndsWith(Constants.CodeSnippets.FileExtension));
			if (snippetResources == null)
				return false;

			bool deployedAny = false;
			bool allSnippetsDeployed = true;

			foreach (string snippetResourceName in snippetResources)
			{
				deployedAny = true;
				allSnippetsDeployed = DeploySnippetResource(currentAssembly, snippetResourceName) && allSnippetsDeployed;
			}

			return deployedAny && allSnippetsDeployed;
		}

		private bool DeploySnippetResource(Assembly currentAssembly, string snippetResourceName)
		{
			string snippetFilePath = TransformAssemblyResourceNameToFilePath(snippetResourceName);
			string snippetDirectory = Path.GetDirectoryName(snippetFilePath);

			if (!StorageUtils.CreateDirectory(snippetDirectory))
				return false;

			try
			{
				using (Stream resourceStream = currentAssembly.GetManifestResourceStream(snippetResourceName))
				{
					if (resourceStream == null)
						return false;

					using (var fileStream = new FileStream(snippetFilePath, FileMode.Create))
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
			const string namespaceResourcePrefix = "Acuminator.Vsix.Code_Snippets.";

			string relativeFilePathWithoutExtension = resourceName.Remove(resourceName.Length - Constants.CodeSnippets.FileExtension.Length)
																  .Remove(0, namespaceResourcePrefix.Length)
																  .Replace('_', ' ')
																  .Replace('.', Path.DirectorySeparatorChar);
			string relativeFilePath = Path.ChangeExtension(relativeFilePathWithoutExtension, Constants.CodeSnippets.FileExtension);
			return Path.Combine(_snippetsFolder, relativeFilePath);
		}
	}
}
