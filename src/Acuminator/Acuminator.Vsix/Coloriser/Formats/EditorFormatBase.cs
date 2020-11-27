using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Media;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Acuminator.Vsix.Utilities;

using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
using VSConstants =  Microsoft.VisualStudio.VSConstants;


namespace Acuminator.Vsix.Coloriser
{
    internal abstract class EditorFormatBase : ClassificationFormatDefinition, IDisposable
    {
        private const string TextCategory = "text";
        private readonly string _classificationTypeName;

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

			if (_classificationFormatMapService == null || _classificationRegistry == null ||  _classificationTypeName == null)
                return;

            var fontAndColorStorage = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();
            var fontAndColorCacheManager = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();

            if (fontAndColorStorage == null || fontAndColorCacheManager == null)
                return;

            Guid guidTextEditorFontCategory = DefGuidList.guidTextEditorFontCategory;
            fontAndColorCacheManager.CheckCache(ref guidTextEditorFontCategory, out int _);

            if (fontAndColorStorage.OpenCategory(ref guidTextEditorFontCategory, (uint) __FCSTORAGEFLAGS.FCSF_READONLY) != VSConstants.S_OK)
            {
                //TODO Log error              
            }

            Color? foregroundColorForTheme =  VSColors.GetThemedColor(_classificationTypeName);

            if (foregroundColorForTheme == null)
                return;
                    
            IClassificationFormatMap formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: TextCategory);

            if (formatMap == null)
                return;

            try
            {
                formatMap.BeginBatchUpdate();
                ForegroundColor = foregroundColorForTheme;
                var bqlOperatorClasType = _classificationRegistry.GetClassificationType(_classificationTypeName);

                if (bqlOperatorClasType == null)
                    return;

                ColorableItemInfo[] colorInfo = new ColorableItemInfo[1];

                if (fontAndColorStorage.GetItem(_classificationTypeName, colorInfo) != VSConstants.S_OK)    //comment from F# repo: "we don't touch the changes made by the user"
                {
                    var properties = formatMap.GetTextProperties(bqlOperatorClasType);
                    var newProperties = properties.SetForeground(ForegroundColor.Value);

                    formatMap.SetTextProperties(bqlOperatorClasType, newProperties);
                }                                                                           
            }
            catch (Exception)
            {
                //TODO Log error here               
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }          
        }

        void IDisposable.Dispose()
        {
            VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
        }
    }
}
