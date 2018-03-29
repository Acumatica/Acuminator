using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acuminator.Vsix.Resources;
using Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix
{
	public class GeneralOptionsPage : DialogPage
	{
		private const string AllSettings = "All";

		private bool colorSettingsChanged;
		public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
		public const string PageTitle = "General";

		private bool coloringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Description))]
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

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Description))]
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

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Description))]
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

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Description))]
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

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Description))]
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

		[CategoryFromResources(nameof(VSIXResource.Category_Acuminator), AcuminatorVSPackage.SettingsCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Description))]
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
