using System;

namespace Acuminator.Vsix.Settings
{
    /// <summary>
    /// Interface for Acuminator settings events.
    /// </summary>
    public interface ISettingsEvents
	{
		public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
		public event EventHandler<SettingChangedEventArgs> CodeAnalysisSettingChanged;
	}
}
