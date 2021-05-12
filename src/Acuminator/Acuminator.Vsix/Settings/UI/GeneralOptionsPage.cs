using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acuminator.Utilities;
using Acuminator.Vsix.Settings;

using Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix
{
	public class GeneralOptionsPage : DialogPage, ISettingsEvents
	{
		public const string PageTitle = "General";

		private const string AllSettings = "All";
		private const string ColoringCategoryName = "BQL Coloring";
		private const string OutliningCategoryName = "BQL Outlining";
		private const string CodeAnalysisCategoryName = "Code Analysis";

		private bool _colorSettingsChanged;
		private bool _codeAnalysisSettingsChanged;
		public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
		public event EventHandler<SettingChangedEventArgs> CodeAnalysisSettingChanged;
		
		private bool _coloringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Description))]
		public bool ColoringEnabled
		{
			get => _coloringEnabled;
			set
			{
				if (_coloringEnabled != value)
				{
					_coloringEnabled = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _pxActionColoringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Description))]
		public bool PXActionColoringEnabled
		{
			get => _pxActionColoringEnabled;
			set
			{
				if (_pxActionColoringEnabled != value)
				{
					_pxActionColoringEnabled = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _pxGraphColoringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Description))]
		public bool PXGraphColoringEnabled
		{
			get => _pxGraphColoringEnabled;
			set
			{
				if (_pxGraphColoringEnabled != value)
				{
					_pxGraphColoringEnabled = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _colorOnlyInsideBQL;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Description))]
		public bool ColorOnlyInsideBQL
		{
			get => _colorOnlyInsideBQL;
			set
			{
				if (_colorOnlyInsideBQL != value)
				{
					_colorOnlyInsideBQL = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _useRegexColoring;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Description))]
		public bool UseRegexColoring
		{
			get => _useRegexColoring;
			set
			{
				if (_useRegexColoring != value)
				{
					_useRegexColoring = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _useBqlOutlining = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Outlining), OutliningCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Description))]
		public bool UseBqlOutlining
		{
			get => _useBqlOutlining;
			set
			{
				if (_useBqlOutlining != value)
				{
					_useBqlOutlining = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _useBqlDetailedOutlining = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Outlining), OutliningCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlDetailedOutlining_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlDetailedOutlining_Description))]
		public bool UseBqlDetailedOutlining
		{
			get => _useBqlDetailedOutlining;
			set
			{
				if (_useBqlDetailedOutlining != value)
				{
					_useBqlDetailedOutlining = value;
					_colorSettingsChanged = true;
				}
			}
		}

		private bool _staticAnalysisEnabled = CodeAnalysisSettings.DefaultStaticAnalysisEnabled;

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_StaticAnalysisEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_StaticAnalysisEnabled_Description))]
		public bool StaticAnalysisEnabled
		{
			get => _staticAnalysisEnabled;
			set
			{
				if (_staticAnalysisEnabled != value)
				{
					_staticAnalysisEnabled = value;
					_codeAnalysisSettingsChanged = true;
				}
			}
		}

		private bool _suppressionMechanismEnabled = CodeAnalysisSettings.DefaultSuppressionMechanismEnabled;

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_SuppressionMechanismEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_SuppressionMechanismEnabled_Description))]
		public bool SuppressionMechanismEnabled
		{
			get => _suppressionMechanismEnabled;
			set
			{
				if (_suppressionMechanismEnabled != value)
				{
					_suppressionMechanismEnabled = value;
					_codeAnalysisSettingsChanged = true;
				}
			}
		}

		private bool _recursiveAnalysisEnabled = CodeAnalysisSettings.DefaultRecursiveAnalysisEnabled;

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_RecursiveAnalysisEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_RecursiveAnalysisEnabled_Description))]
		public bool RecursiveAnalysisEnabled
		{
			get => _recursiveAnalysisEnabled;
			set
			{
				if (_recursiveAnalysisEnabled != value)
				{
					_recursiveAnalysisEnabled = value;
					_codeAnalysisSettingsChanged = true;
				}
			}
		}

		private bool _isvSpecificAnalyzersEnabled = CodeAnalysisSettings.DefaultISVSpecificAnalyzersEnabled;

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_IsvSpecificAnalyzersEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_IsvSpecificAnalyzersEnabled_Description))]
		public bool IsvSpecificAnalyzersEnabled
		{
			get => _isvSpecificAnalyzersEnabled;
			set
			{
				if (_isvSpecificAnalyzersEnabled != value)
				{
					_isvSpecificAnalyzersEnabled = value;
					_codeAnalysisSettingsChanged = true;
				}
			}
		}

		private bool _px1007DocumentationDiagnosticEnabled = CodeAnalysisSettings.DefaultPX1007DocumentationDiagnosticEnabled;

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_PX1007DiagnosticEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_PX1007DiagnosticEnabled_Description))]
		public bool PX1007DocumentationDiagnosticEnabled
		{
			get => _px1007DocumentationDiagnosticEnabled;
			set
			{
				if (_px1007DocumentationDiagnosticEnabled != value)
				{
					_px1007DocumentationDiagnosticEnabled = value;
					_codeAnalysisSettingsChanged = true;
				}
			}
		}

		public override void ResetSettings()
		{
			_coloringEnabled = true;
			_useRegexColoring = false;
			_useBqlOutlining = true;
			_useBqlDetailedOutlining = true;
			_pxActionColoringEnabled = true;
			_pxGraphColoringEnabled = true;
			_colorOnlyInsideBQL = false;

			_staticAnalysisEnabled = CodeAnalysisSettings.DefaultStaticAnalysisEnabled;
			_suppressionMechanismEnabled = CodeAnalysisSettings.DefaultSuppressionMechanismEnabled;
			_recursiveAnalysisEnabled = CodeAnalysisSettings.DefaultRecursiveAnalysisEnabled;
			_isvSpecificAnalyzersEnabled = CodeAnalysisSettings.DefaultISVSpecificAnalyzersEnabled;
			_px1007DocumentationDiagnosticEnabled = CodeAnalysisSettings.DefaultPX1007DocumentationDiagnosticEnabled;
			
			_colorSettingsChanged = false;
			_codeAnalysisSettingsChanged = false;

			base.ResetSettings();

			OnColoringSettingChanged(AllSettings);
			OnCodeAnalysisSettingChanged(AllSettings);
		}

		public override void SaveSettingsToStorage()
		{
			base.SaveSettingsToStorage();

			if (_colorSettingsChanged)
			{
				_colorSettingsChanged = false;
				OnColoringSettingChanged(AllSettings);
			}

			if (_codeAnalysisSettingsChanged)
			{
				_codeAnalysisSettingsChanged = false;
				OnCodeAnalysisSettingChanged(AllSettings);
			}
		}

		private void OnColoringSettingChanged(string setting)
		{
			ColoringSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
		}

		private void OnCodeAnalysisSettingChanged(string setting)
		{
			CodeAnalysisSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
		}
	}
}
