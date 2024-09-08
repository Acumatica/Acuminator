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
		private const string WhiteListWatcherDescription = "White List API";

		private readonly GeneralOptionsPage _optionsPage;
		private readonly ApiFileSettingWatcher _bannedApiFileWatcher;
		private readonly ApiFileSettingWatcher _whiteListApiFileWatcher;

		public BannedApiSettingsObserver(GeneralOptionsPage optionsPage)
		{
			_optionsPage = optionsPage.CheckIfNull();

			_optionsPage.BannedApiSettingChanged += BannedApiSettingChanged;

			_bannedApiFileWatcher = new ApiFileSettingWatcher(BannedApiWatcherDescription, optionsPage, 
											(options, newFilePath, raiseEvents) => options.SetBannedApiFilePathExternally(newFilePath, raiseEvents));
			_whiteListApiFileWatcher = new ApiFileSettingWatcher(WhiteListWatcherDescription, optionsPage,
											(options, newFilePath, raiseEvents) => options.SetWhiteListFilePathExternally(newFilePath, raiseEvents));
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
				case Constants.Settings.WhiteListApiFilePath:
					_whiteListApiFileWatcher.UpdateApiFileWatcher(_optionsPage.WhiteListApiFilePath);
					return;
				case Constants.Settings.All:
					_bannedApiFileWatcher.UpdateApiFileWatcher(_optionsPage.BannedApiFilePath);
					_whiteListApiFileWatcher.UpdateApiFileWatcher(_optionsPage.WhiteListApiFilePath);
					return;
				default:
					return;
			}
		}
	}
}
