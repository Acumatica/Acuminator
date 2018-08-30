using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix
{
    public class SettingChangedEventArgs : EventArgs
    {
        public string SettingName { get; }

        public SettingChangedEventArgs(string settingName) 
        {
            settingName.ThrowOnNullOrWhiteSpace(nameof(settingName));

            SettingName = settingName;
        }
    }
}
