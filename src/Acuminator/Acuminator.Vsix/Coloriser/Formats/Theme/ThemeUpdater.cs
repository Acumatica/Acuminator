using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;

using VSConstants =  Microsoft.VisualStudio.VSConstants;


namespace Acuminator.Vsix.Coloriser
{
    internal class ThemeUpdater
    {
        private const string TextCategory = "text";
        private const string MefItemsGuidString = "75A05685-00A8-4DED-BAE5-E7A50BFA929A";
        private Guid _mefItemsGuid = new Guid(MefItemsGuidString);

        private IClassificationFormatMapService _classificationFormatMapService;  
        private IClassificationTypeRegistryService _classificationRegistry; 
        private IVsFontAndColorStorage _fontAndColorStorage; 
        private IVsFontAndColorCacheManager _fontAndColorCacheManager;

        private readonly IServiceProvider _serviceProvider;

        public event EventHandler<AcuminatorThemeChangedEventArgs> AcuminatorThemeChanged;

        public static ThemeUpdater Instance { get; } = new ThemeUpdater();

        private ThemeUpdater()
        {
			ThreadHelper.ThrowIfNotOnUIThread();

            _serviceProvider = (AcuminatorVSPackage.Instance as IServiceProvider) ?? ServiceProvider.GlobalProvider;
            _serviceProvider.ThrowOnNull(nameof(_serviceProvider));

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;         
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_classificationFormatMapService == null || _classificationRegistry == null)
			{
                var componentModel = _serviceProvider.GetService<SComponentModel, IComponentModel>();

                _classificationFormatMapService ??= componentModel?.GetService<IClassificationFormatMapService>();
                _classificationRegistry ??= componentModel?.GetService<IClassificationTypeRegistryService>();
            }

            if (_classificationFormatMapService == null || _classificationRegistry == null)
                return;

             _fontAndColorStorage ??= _serviceProvider.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();
             _fontAndColorCacheManager ??= _serviceProvider.GetService<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();
            IClassificationFormatMap formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: TextCategory);

            if (_fontAndColorStorage == null || _fontAndColorCacheManager == null || formatMap == null)
                return;

            _fontAndColorCacheManager.CheckCache(ref _mefItemsGuid, out int _);
            int openCategoryResult = _fontAndColorStorage.OpenCategory(ref _mefItemsGuid, (uint)__FCSTORAGEFLAGS.FCSF_READONLY);

            if (openCategoryResult != VSConstants.S_OK)
            {
                AcuminatorLogger.LogMessage($"Error on opening category in the registry during the theme change. The error code is {openCategoryResult}",
											LogMode.Error);             
            }

            try
            {
                var acuminatorThemeChangedEventArgs = new AcuminatorThemeChangedEventArgs(_fontAndColorStorage, _classificationRegistry, formatMap);

                formatMap.BeginBatchUpdate();
                AcuminatorThemeChanged?.Invoke(this, acuminatorThemeChangedEventArgs);
            }
            catch (Exception exception)
            {
				AcuminatorLogger.LogException(exception, LogMode.Error, addErrorPrefix: true);
            }
            finally
            {
                formatMap.EndBatchUpdate();
                int refreshCacheResult = _fontAndColorCacheManager.RefreshCache(ref _mefItemsGuid);

                if (refreshCacheResult != VSConstants.S_OK)
				{
					AcuminatorLogger.LogMessage($"Error on the refresh of MEF Items cache in the registry during the theme change. The error code is {refreshCacheResult}",
												LogMode.Error);
                }

                _fontAndColorStorage.CloseCategory();
            }
        }
    }
}
