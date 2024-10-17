#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
	[Export(typeof(BannedApiSettings))]
	internal class BannedApiSettingsFromOptionsPage : BannedApiSettings
	{
		private readonly GeneralOptionsPage _optionsPage;

		[ImportingConstructor]
		public BannedApiSettingsFromOptionsPage(GeneralOptionsPage optionsPage)
		{
			_optionsPage = optionsPage.CheckIfNull();
		}

		public override bool BannedApiAnalysisEnabled => _optionsPage.BannedApiAnalysisEnabled;

		public override string? BannedApiFilePath => _optionsPage.BannedApiFilePath;

		public override string? AllowedApisFilePath => _optionsPage.WhiteListApiFilePath;
	}
}
