using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using PX.Analyzers.Vsix;
using PX.Analyzers.Vsix.Utilities;

namespace PX.Analyzers.Coloriser
{
    internal abstract class EditorFormatBase : ClassificationFormatDefinition
    {
        private const string textCategory = "text";
        private readonly string classificationTypeName; 
        
        protected EditorFormatBase()
        {          
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            Type type = this.GetType();
            classificationTypeName = type.GetCustomAttribute<NameAttribute>()?.Name;
            
            if (classificationTypeName != null)
            {
                ForegroundColor = VSColors.GetThemedColor(classificationTypeName);
            }
        }      

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            if (AcuminatorVSPackage.Instance?.ClassificationFormatMapService == null ||
                AcuminatorVSPackage.Instance.ClassificationRegistry == null ||
                classificationTypeName == null)
            {
                return;
            }
            
            Color? foregroundColorForTheme =  VSColors.GetThemedColor(classificationTypeName);

            if (foregroundColorForTheme == null)
                return;

            IClassificationFormatMap formatMap = AcuminatorVSPackage.Instance.ClassificationFormatMapService
                                                                             .GetClassificationFormatMap(category: textCategory);

            if (formatMap == null)
                return;

            var bqlOperatorClasType = AcuminatorVSPackage.Instance.ClassificationRegistry
                                                                  .GetClassificationType(classificationTypeName);

            if (bqlOperatorClasType == null)
                return;

            try
            {                            
                ForegroundColor = foregroundColorForTheme;
                var properties = formatMap.GetTextProperties(bqlOperatorClasType);
                var newProperties = properties.SetForeground(ForegroundColor.Value);

                formatMap.SetTextProperties(bqlOperatorClasType, newProperties);
            }
            catch (Exception)
            {
                //TODO Log error here               
            }           
        }
    }
}
