using System;

namespace Acuminator.Vsix.Settings
{
    /// <summary>
    /// Interface for Acuminator settings events.
    /// </summary>
    internal interface ISettingsEvents
	{
		public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
		public event EventHandler<SettingChangedEventArgs> CodeAnalysisSettingChanged;
	}
}
