#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.BannedApi
{

	/// <summary>
	/// A banned API settings observer. Currently unused by may be useful in the future.
	/// </summary>
	internal class BannedApiSettingsObserver : IDisposable
	{
		private const string BannedApiWatcherDescription = "Banned API";
		private const string AllowedApiWatcherDescription = "Allowed API";

		private readonly GeneralOptionsPage _optionsPage;
		private readonly ApiFileSettingWatcher _bannedApiFileWatcher;
		private readonly ApiFileSettingWatcher _allowedApiFileWatcher;

		public BannedApiSettingsObserver(GeneralOptionsPage optionsPage)
		{
			_optionsPage = optionsPage.CheckIfNull();

			_optionsPage.BannedApiSettingChanged += BannedApiSettingChanged;

			_bannedApiFileWatcher = new ApiFileSettingWatcher(BannedApiWatcherDescription, optionsPage, 
											(options, newFilePath, raiseEvents) => options.SetBannedApiFilePathExternally(newFilePath, raiseEvents));
			_allowedApiFileWatcher = new ApiFileSettingWatcher(AllowedApiWatcherDescription, optionsPage,
											(options, newFilePath, raiseEvents) => options.SetAllowedApiFilePathExternally(newFilePath, raiseEvents));
		}

		public void Dispose()
		{
			_optionsPage.BannedApiSettingChanged -= BannedApiSettingChanged;

			_bannedApiFileWatcher.Dispose();
			_bannedApiFileWatcher.Dispose();
		}

		private void BannedApiSettingChanged(object sender, SettingChangedEventArgs e)
		{
			switch (e.SettingName)
			{
				case Constants.Settings.BannedApiFilePath:
					_bannedApiFileWatcher.UpdateApiFileWatcher(_optionsPage.BannedApiFilePath);
					return;
				case Constants.Settings.AllowedApiFilePath:
					_allowedApiFileWatcher.UpdateApiFileWatcher(_optionsPage.AllowedApiFilePath);
					return;
				case Constants.Settings.All:
					_bannedApiFileWatcher.UpdateApiFileWatcher(_optionsPage.BannedApiFilePath);
					_allowedApiFileWatcher.UpdateApiFileWatcher(_optionsPage.AllowedApiFilePath);
					return;
				default:
					return;
			}
		}
	}
}
