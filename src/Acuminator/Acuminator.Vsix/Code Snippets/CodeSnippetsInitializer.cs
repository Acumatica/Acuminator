
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using System.Runtime.CompilerServices;

namespace Acuminator.Vsix.CodeSnippets
{
    /// <summary>
    /// Code Snippets initializing logic
    /// </summary>
    internal class CodeSnippetsInitializer
	{
		private readonly SnippetsVersionFile _snippetsVersionFile = new SnippetsVersionFile();

		public string? SnippetsFolder { get; }

		public bool IsSnippetsFolderInitialized => SnippetsFolder != null;

		public CodeSnippetsInitializer()
		{
			string? snippetsRootFolder;

			try
			{
				snippetsRootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return;
			}

			if (!snippetsRootFolder.IsNullOrWhiteSpace() && Directory.Exists(snippetsRootFolder))
			{
				SnippetsFolder = $@"{snippetsRootFolder}\Acuminator\Acumatica Code Snippets";
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
			if (!EnsureDirectoryExists(SnippetsFolder!))
				return false;

			if (!_snippetsVersionFile.WriteVersionFile(SnippetsFolder!, version))
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

			if (!EnsureDirectoryExists(snippetDirectory))
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

		private bool EnsureDirectoryExists(string directory)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string TransformAssemblyResourceNameToFilePath(string resourceName) =>
			resourceName.Replace('_', ' ')
						.Replace('.', Path.DirectorySeparatorChar);
	}
}
