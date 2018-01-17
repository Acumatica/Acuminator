using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Analyzers.Coloriser;

namespace PX.Analyzers.Vsix
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
