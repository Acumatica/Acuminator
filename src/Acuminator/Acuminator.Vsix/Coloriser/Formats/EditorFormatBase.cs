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
using Acuminator.Vsix.Utilities;

using VSConstants =  Microsoft.VisualStudio.VSConstants;


namespace Acuminator.Vsix.Coloriser
{
    internal abstract class EditorFormatBase : ClassificationFormatDefinition, IDisposable
    {
        private const string TextCategory = "text";
        private readonly string _classificationTypeName;

        private const string MefItemsGuidString = "75A05685-00A8-4DED-BAE5-E7A50BFA929A";
        private Guid _mefItemsGuid = new Guid(MefItemsGuidString);

        [Import]
        internal IClassificationFormatMapService _classificationFormatMapService = null;  //Set via MEF

        [Import]
        internal IClassificationTypeRegistryService _classificationRegistry = null; // Set via MEF

        protected EditorFormatBase()
        {          
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            Type type = this.GetType();
            _classificationTypeName = type.GetCustomAttribute<NameAttribute>()?.Name;
            
            if (_classificationTypeName != null)
            {
                ForegroundColor = VSColors.GetThemedColor(_classificationTypeName);
            }
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_classificationFormatMapService == null || _classificationRegistry == null || _classificationTypeName == null)
                return;

            var fontAndColorStorage = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();
            var fontAndColorCacheManager = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();

            if (fontAndColorStorage == null || fontAndColorCacheManager == null)
                return;

            fontAndColorCacheManager.CheckCache(ref _mefItemsGuid, out int _);

            if (fontAndColorStorage.OpenCategory(ref _mefItemsGuid, (uint)__FCSTORAGEFLAGS.FCSF_READONLY) != VSConstants.S_OK)
            {
                //TODO Log error              
            }

            Color? foregroundColorForTheme = VSColors.GetThemedColor(_classificationTypeName);

            if (foregroundColorForTheme == null)
                return;

            IClassificationFormatMap formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: TextCategory);

            if (formatMap == null)
                return;

            try
            {
                formatMap.BeginBatchUpdate();
                ForegroundColor = foregroundColorForTheme;
                var classificationType = _classificationRegistry.GetClassificationType(_classificationTypeName);

                if (classificationType == null)
                    return;

                ColorableItemInfo[] colorInfo = new ColorableItemInfo[1];

                if (fontAndColorStorage.GetItem(_classificationTypeName, colorInfo) != VSConstants.S_OK)    //comment from F# repo: "we don't touch the changes made by the user"
                {
                    var properties = formatMap.GetTextProperties(classificationType);
                    var newProperties = properties.SetForeground(ForegroundColor.Value);

                    formatMap.SetTextProperties(classificationType, newProperties);
                }      
            }
            catch (Exception)
            {
                //TODO Log error here               
            }
            finally
            {
                formatMap.EndBatchUpdate();
                
				if (fontAndColorCacheManager.RefreshCache(ref _mefItemsGuid) != VSConstants.S_OK)
				{
					fontAndColorCacheManager.ClearCache(ref _mefItemsGuid);
				}

                fontAndColorStorage.CloseCategory();
            }
        }

        void IDisposable.Dispose()
        {
            VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
        }
    }
}
