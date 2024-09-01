#nullable enable

using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Community.VisualStudio.Toolkit;

using Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix
{
	[ComVisible(true)]
	public class GeneralOptionsPage : DialogPage, ISettingsEvents
	{
		public const string PageTitle = "General";

		private const string AllSettings = "All";
		private const string ColoringCategoryName = "BQL Coloring";
		private const string OutliningCategoryName = "BQL Outlining";
		private const string CodeAnalysisCategoryName = "Code Analysis";
		private const string BannedApiCategoryName = "Banned API";

		private bool _colorSettingsChanged;
		private bool _bannedApiFileInvalid, _whiteListApiFileInvalid;
		private bool _codeAnalysisSettingsChanged;
		public event EventHandler<SettingChangedEventArgs>? ColoringSettingChanged;
		public event EventHandler<SettingChangedEventArgs>? CodeAnalysisSettingChanged;
		
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

		private string? _bannedApiFilePath;

		[CategoryFromResources(nameof(VSIXResource.Category_BannedAPI), BannedApiCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_BannedApiFilePath_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_BannedApiFilePath_Description))]
		public string? BannedApiFilePath
		{
			get => _bannedApiFilePath;
			set 
			{
				if (_bannedApiFilePath != value)
				{
					_bannedApiFilePath = value.NullIfWhiteSpace()?.Trim();
					_codeAnalysisSettingsChanged = true;
					_bannedApiFileInvalid = !CheckFilePath(_bannedApiFilePath, VSIXResource.Setting_CodeAnalysis_BannedApiFilePath_Title);
				}
			}
		}

		private string? _whiteListApiFilePath;

		[CategoryFromResources(nameof(VSIXResource.Category_BannedAPI), BannedApiCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_WhiteListApiFilePath_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_WhiteListApiFilePath_Description))]
		public string? WhiteListApiFilePath
		{
			get => _whiteListApiFilePath;
			set 
			{
				if (_whiteListApiFilePath != value)
				{
					_whiteListApiFilePath = value.NullIfWhiteSpace()?.Trim();
					_codeAnalysisSettingsChanged = true;
					_whiteListApiFileInvalid = !CheckFilePath(_whiteListApiFilePath, VSIXResource.Setting_CodeAnalysis_WhiteListApiFilePath_Title);
				}
			}
		}

		public override void ResetSettings()
		{
			_coloringEnabled 		 = true;
			_useRegexColoring 		 = false;
			_useBqlOutlining 		 = true;
			_useBqlDetailedOutlining = true;
			_pxActionColoringEnabled = true;
			_pxGraphColoringEnabled  = true;
			_colorOnlyInsideBQL 	 = false;

			_staticAnalysisEnabled 				  = CodeAnalysisSettings.DefaultStaticAnalysisEnabled;
			_suppressionMechanismEnabled 		  = CodeAnalysisSettings.DefaultSuppressionMechanismEnabled;
			_recursiveAnalysisEnabled 			  = CodeAnalysisSettings.DefaultRecursiveAnalysisEnabled;
			_isvSpecificAnalyzersEnabled 		  = CodeAnalysisSettings.DefaultISVSpecificAnalyzersEnabled;
			_px1007DocumentationDiagnosticEnabled = CodeAnalysisSettings.DefaultPX1007DocumentationDiagnosticEnabled;

			_bannedApiFilePath 	  = null;
			_whiteListApiFilePath = null;
			
			_colorSettingsChanged 		 = false;
			_codeAnalysisSettingsChanged = false;
			_whiteListApiFileInvalid 	 = false;
			_bannedApiFileInvalid 		 = false;

			base.ResetSettings();

			OnColoringSettingChanged(AllSettings);
			OnCodeAnalysisSettingChanged(AllSettings);
		}

		public override void SaveSettingsToStorage()
		{
			if (_bannedApiFileInvalid)
			{
				_bannedApiFilePath 	  = null;
				_bannedApiFileInvalid = false;
			}

			if (_whiteListApiFileInvalid)
			{
				_whiteListApiFilePath 	 = null;
				_whiteListApiFileInvalid = false;
			}

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

		public void SetDeployedBannedApiSettings(string? deployedBannedApisFile, string? deployedWhiteListFile)
		{
			if (!deployedBannedApisFile.IsNullOrWhiteSpace() && BannedApiFilePath.IsNullOrWhiteSpace() &&
				File.Exists(deployedBannedApisFile))
			{
				_bannedApiFilePath = deployedBannedApisFile;
				_codeAnalysisSettingsChanged = true;
			}

			if (!deployedWhiteListFile.IsNullOrWhiteSpace() && WhiteListApiFilePath.IsNullOrWhiteSpace())
			{
				_whiteListApiFilePath = deployedWhiteListFile;
				_codeAnalysisSettingsChanged = true;
			}

			if (_codeAnalysisSettingsChanged)
				SaveSettingsToStorage();
		}

		private void OnColoringSettingChanged(string setting)
		{
			ColoringSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
		}

		private void OnCodeAnalysisSettingChanged(string setting)
		{
			CodeAnalysisSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
		}

		private static bool CheckFilePath(string? filePath, string settingName)
		{
			if (filePath.IsNullOrWhiteSpace())
				return true;

			if (!File.Exists(filePath))
			{
				string errorMessage = string.Format(VSIXResource.Settings_InvalidFileErrorFormat, settingName);
				VS.MessageBox.ShowWarning(VSIXResource.Settings_InvalidFileErrorCaption, errorMessage);

				return false;
			}

			return true;
		}
	}
}
