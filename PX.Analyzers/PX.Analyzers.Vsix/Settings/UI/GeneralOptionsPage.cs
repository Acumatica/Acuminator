using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace PX.Analyzers.Vsix
{
    public class GeneralOptionsPage : DialogPage
    {
        public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
        public const string PageTitle = "General";

        private bool coloringEnabled = true;

        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("Coloring enabled")]
        [Description("Syntax coloring enabled")]
        public bool ColoringEnabled
        {
            get => coloringEnabled;
            set
            {
                if (coloringEnabled != value)
                {
                    coloringEnabled = value;
                    OnSettingsChanged(nameof(ColoringEnabled));
                }
            }
        }

        private bool useRegexColoring;

        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("Use RegEx coloriser")]
        [Description("Use syntax coloriser implemented via regular expressions, provide worse coloring but works faster")]
        public bool UseRegexColoring
        {
            get => useRegexColoring;
            set
            {
                if (useRegexColoring != value)
                {
                    useRegexColoring = value;
                    OnSettingsChanged(nameof(UseRegexColoring));
                }
            }
        }

        private void OnSettingsChanged(string setting)
        {
            ColoringSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
        }
    }
}
