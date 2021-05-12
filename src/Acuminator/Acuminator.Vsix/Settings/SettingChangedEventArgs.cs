using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
    public class SettingChangedEventArgs : EventArgs
    {
        public string SettingName { get; }

        public SettingChangedEventArgs(string settingName) 
        {
            SettingName = settingName.CheckIfNullOrWhiteSpace(nameof(settingName));
        }
    }
}
