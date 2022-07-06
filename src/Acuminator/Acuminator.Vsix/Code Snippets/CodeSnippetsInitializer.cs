
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using Microsoft.VisualStudio.Shell;

using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.CodeSnippets
{
    /// <summary>
    /// Code Snippets initializing logic
    /// </summary>
    public class CodeSnippetsInitializer
	{
		private readonly SnippetsVersionFile _snippetsVersionFile = new SnippetsVersionFile();
		public string? SnippetsFolder { get; }

		public bool IsSnippetsFolderInitialized => SnippetsFolder != null;

		public CodeSnippetsInitializer()
		{
			string? myDocumentsFolder;

			try
			{
				myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return;
			}

			if (myDocumentsFolder.IsNullOrWhiteSpace() || !Directory.Exists(myDocumentsFolder))
			{
				AcuminatorLogger.LogException(new Exception("User \"Documents\" folder was not found on the machine"));
				return;
			}

			try
			{
				SnippetsFolder = Path.Combine(myDocumentsFolder, AcuminatorVSPackage.PackageName, Constants.CodeSnippets.CodeSnippetsFolder);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return;
			}
		}

		/// <summary>
		/// Initialize code snippets.
		/// </summary>
		/// <param name="packageVersion">The package version.</param>
		/// <returns>
		/// True if it succeeds, false if it fails.
		/// </returns>
		public bool InitializeCodeSnippets(Version packageVersion)
		{
			packageVersion.ThrowOnNull(nameof(packageVersion));
			
			if (!IsSnippetsFolderInitialized)
				return false;

			Version? existingVersion = _snippetsVersionFile.TryGetExistingSnippetsVersion(SnippetsFolder);

			if (existingVersion != null && packageVersion <= existingVersion)
				return true;

			return DeployCodeSnippets(packageVersion);
		}

		private bool DeployCodeSnippets(Version version)
		{
			if (!ReCreateDirectory(SnippetsFolder!))
				return false;

			if (!DeployCodeSnippetsFromAssemblyResources())
				return false;

			return _snippetsVersionFile.WriteVersionFile(SnippetsFolder!, version); 
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

			if (!CreateDirectory(snippetDirectory))
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

		private bool ReCreateDirectory(string directory)
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

		private bool CreateDirectory(string directory)
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

		internal string TransformAssemblyResourceNameToFilePath(string resourceName)
		{
			const string namespaceResourcePrefix = "Acuminator.Vsix.Code_Snippets.";

			string relativeFilePathWithoutExtension = resourceName.Remove(resourceName.Length - Constants.CodeSnippets.FileExtension.Length)
																  .Remove(0, namespaceResourcePrefix.Length)
																  .Replace('_', ' ')
																  .Replace('.', Path.DirectorySeparatorChar);
			string relativeFilePath = Path.ChangeExtension(relativeFilePathWithoutExtension, Constants.CodeSnippets.FileExtension);
			return Path.Combine(SnippetsFolder, relativeFilePath);
		}
	}
}
