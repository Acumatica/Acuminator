using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;

namespace PX.Analyzers.Vsix
{
    public class GeneralOptionsPage : DialogPage
    {
        private const string AllSettings = "All"; 

        private bool colorSettingsChanged;
        public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
        public const string PageTitle = "General";

        private bool coloringEnabled = true;
		
        [AcuCategory(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
        [AcuDisplayName(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Title))]
        [AcuDescription(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Description))]
        public bool ColoringEnabled
        {
            get => coloringEnabled;
            set
            {
                if (coloringEnabled != value)
                {
                    coloringEnabled = value;
                    colorSettingsChanged = true;
                }
            }
        }

        private bool pxActionColoringEnabled = true;
		
        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("PXAction coloring is enabled")]
        [Description("Coloring for PXAction declarations is enabled for Roslyn coloring")]
        public bool PXActionColoringEnabled
        {
            get => pxActionColoringEnabled;
            set
            {
                if (pxActionColoringEnabled != value)
                {
                    pxActionColoringEnabled = value;
                    colorSettingsChanged = true;
                }
            }
        }

        private bool pxGraphColoringEnabled = true;

        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("PXGraph coloring is enabled")]
        [Description("Coloring for PXGraph declarations is enabled for Roslyn coloring")]
        public bool PXGraphColoringEnabled
        {
            get => pxGraphColoringEnabled;
            set
            {
                if (pxGraphColoringEnabled != value)
                {
                    pxGraphColoringEnabled = value;
                    colorSettingsChanged = true;
                }
            }
        }

        private bool colorOnlyInsideBQL;

        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("Color code only inside BQL")]
        [Description("Coloring is enabled only inside BQL commands for Roslyn coloring")]
        public bool ColorOnlyInsideBQL
        {
            get => colorOnlyInsideBQL;
            set
            {
                if (colorOnlyInsideBQL != value)
                {
                    colorOnlyInsideBQL = value;
                    colorSettingsChanged = true;
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
                    colorSettingsChanged = true;
                }
            }
        }

        private bool useBqlOutlining = true;

        [Category(AcuminatorVSPackage.SettingsCategoryName)]
        [DisplayName("Use BQL Outlining")]
        [Description("Use BQL outlining to collapse parts of BQL (works only with Roslyn coloring)")]
        public bool UseBqlOutlining
        {
            get => useBqlOutlining;
            set
            {
                if (useBqlOutlining != value)
                {
                    useBqlOutlining = value;
                    colorSettingsChanged = true;
                }
            }
        }

        public override void ResetSettings()
        {
            coloringEnabled = true;
            useRegexColoring = false;
            useBqlOutlining = true;
            pxActionColoringEnabled = true;
            pxGraphColoringEnabled = true;
            colorOnlyInsideBQL = false;

            colorSettingsChanged = false;
            base.ResetSettings();
            OnSettingsChanged(AllSettings);
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            if (colorSettingsChanged)
            {
                colorSettingsChanged = false;
                OnSettingsChanged(AllSettings);
            }
        }

        private void OnSettingsChanged(string setting)
        {          
            ColoringSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
        }
    }
}
