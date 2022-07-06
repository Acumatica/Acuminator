
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using SnippetConstants = Acuminator.Vsix.Utilities.Constants.CodeSnippets;


namespace Acuminator.Vsix.CodeSnippets
{
	/// <summary>
	/// Code Snippets logic to update VS settings directly in its private registry hive file.
	/// </summary>
	/// <remarks>
	/// VS uses a separate private registry hive file to store its settings independent from other VS installations. 
	/// The settings are grouped into 4 main settings scopes:
	/// <br/><br/>
	/// * User - this is the place where custom user settings can be stored including custom settings from VS extensions<br/>
	/// * Local - VS own setting <br/>
	/// * Remote - also VS own settings, not sure what's the difference with Local<br/>
	/// * Config - if I understand correctly, it is the aggregation of VS settings obtained after combining settings from other scopes.
	/// <br/><br/>
	/// Here are some docs, but only 3 scopes here https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.settings.settingsscope?view=visualstudiosdk-2022)
	/// <br/><br/>
	/// The scopes are implemented as registry subkeys under the <see cref="Package.ApplicationRegistryRoot"/> registry key.
	/// <br/><br/>
	/// VS itself provides high level settings store APIs such as <see cref="Microsoft.VisualStudio.Settings.WritableSettingsStore"/> that should be used in general to read VS settings 
	/// and store custom plugin settings in the User section. However, these APIs provide only read only access to VS own settings in the Config section. 
	/// <br/><br/>
	/// Ther issue with code snippets distribution is that the snippet registration is not updated correctly in the VS Config settings scope. 
	/// After the package uninstallation or upgrade it can become stale.<br/>
	/// Thus we have to use low level Windows Registry API to modify VS own settings to fix this issue.
	/// </remarks>
	internal class CodeSnippetsRegistryUpdater
	{
		private const string CSharpSnippetsRegistrySubKey = @"Languages\CodeExpansions\CSharp\Paths";

		public bool UpdateSnippetsInRegistry(Package package, string snippetsRootFolder)
		{
			snippetsRootFolder.ThrowOnNullOrWhiteSpace(nameof(snippetsRootFolder));

			try
			{
				using (RegistryKey codeSnippetsRegKey = package.ApplicationRegistryRoot.OpenSubKey(CSharpSnippetsRegistrySubKey, writable: true))
				{
					if (codeSnippetsRegKey == null)
						return false;

					RegisterSnippetFolderInVsRegistry(snippetsRootFolder, codeSnippetsRegKey, SnippetConstants.DacSnippetsFolder);
					RegisterSnippetFolderInVsRegistry(snippetsRootFolder, codeSnippetsRegKey, SnippetConstants.GenericGraphEventsFolder);
					RegisterSnippetFolderInVsRegistry(snippetsRootFolder, codeSnippetsRegKey, SnippetConstants.NCGraphEventsFolder);

					return true;
				}
			}
			catch (Exception exception)
			{
				AcuminatorLogger.LogException(exception);
				return false;
			}		
		}

		private void RegisterSnippetFolderInVsRegistry(string snippetsRootFolder, RegistryKey codeSnippetsRegKey, string snippetFolderName)
		{
			string fullFolderPath = Path.Combine(snippetsRootFolder, snippetFolderName);
			codeSnippetsRegKey.SetValue(snippetFolderName, fullFolderPath);
		}
	}
}
