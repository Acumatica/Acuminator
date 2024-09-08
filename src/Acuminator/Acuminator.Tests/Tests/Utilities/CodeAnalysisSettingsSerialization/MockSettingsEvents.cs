#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Vsix.Settings;

namespace Acuminator.Tests.Tests.Utilities.CodeAnalysisSettingsSerialization
{
	internal class MockSettingsEvents : ISettingsEvents
	{
		public event EventHandler<SettingChangedEventArgs>? ColoringSettingChanged
		{
			add { }
			remove { }
		}

		public event EventHandler<SettingChangedEventArgs>? CodeAnalysisSettingChanged
		{
			add { }
			remove { }
		}
	}
}