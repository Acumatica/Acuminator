
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Settings;

using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using SnippetConstants = Acuminator.Vsix.Utilities.Constants.CodeSnippets;

namespace Acuminator.Vsix.CodeSnippets
{
    /// <summary>
    /// Code Snippets logic to update User section of VS settings store
    /// </summary>
    internal class CodeSnippetsSettingsUpdater
	{
		private const string CSharpSnippetsRegistrationStorage = @"Languages\CodeExpansions\CSharp\Paths";

		public async ValueTask<bool> UpdateSnippetsInUserSectionAsync(string snippetsRootFolder, IServiceProvider serviceProvider)
		{
			snippetsRootFolder.ThrowOnNullOrWhiteSpace(nameof(snippetsRootFolder));
			serviceProvider.ThrowOnNull(nameof(serviceProvider));

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			try
			{
				return RewriteSnippetsFoldersInUserSection(snippetsRootFolder, serviceProvider);
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogException(e);
				return false;
			}
		}

		private bool RewriteSnippetsFoldersInUserSection(string snippetsRootFolder, IServiceProvider serviceProvider)
		{
			ShellSettingsManager shellSettingsManager = new ShellSettingsManager(serviceProvider);
			var writableStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

			if (writableStore == null)
			{
				AcuminatorLogger.LogMessage("Failed to obtain writeable settings store", LogMode.Error);
				return false;
			}

			if (!writableStore.CollectionExists(CSharpSnippetsRegistrationStorage))
				writableStore.CreateCollection(CSharpSnippetsRegistrationStorage);

			RegisterSnippetFolderInUserSettings(snippetsRootFolder, writableStore, SnippetConstants.DacSnippetsFolder);
			RegisterSnippetFolderInUserSettings(snippetsRootFolder, writableStore, SnippetConstants.GenericGraphEventsFolder);
			RegisterSnippetFolderInUserSettings(snippetsRootFolder, writableStore, SnippetConstants.NCGraphEventsFolder);
			
			return true;
		}
		
		private void RegisterSnippetFolderInUserSettings(string snippetsRootFolder, WritableSettingsStore writableStore, string snippetFolderName)
		{
			string fullFolderPath = Path.Combine(snippetsRootFolder, snippetFolderName);
			writableStore.SetString(CSharpSnippetsRegistrationStorage, snippetFolderName, fullFolderPath);
		}
	}
}
