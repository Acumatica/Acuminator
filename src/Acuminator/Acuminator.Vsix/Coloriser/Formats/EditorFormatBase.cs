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

using Acuminator.Utilities.Common;

using VSConstants =  Microsoft.VisualStudio.VSConstants;


namespace Acuminator.Vsix.Coloriser
{
    internal abstract class EditorFormatBase : ClassificationFormatDefinition, IDisposable
    {
        private readonly string _classificationTypeName;

        protected EditorFormatBase()
        {
            Type type = this.GetType();
            _classificationTypeName = type.GetCustomAttribute<NameAttribute>()?.Name;
            
            if (_classificationTypeName != null)
            {
                ForegroundColor = VSColors.GetThemedColor(_classificationTypeName);
            }

            AcuminatorVSPackage.Instance.CheckIfNull(nameof(AcuminatorVSPackage))
                               .ThemeUpdater.AcuminatorThemeChanged += AcuminatorThemeChangedHandler;
        }

		private void AcuminatorThemeChangedHandler(object sender, AcuminatorThemeChangedEventArgs e)
		{
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_classificationTypeName == null)
                return;

			try
			{
                Color? foregroundColorForTheme = VSColors.GetThemedColor(_classificationTypeName);

                if (foregroundColorForTheme == null)
                    return;

                ForegroundColor = foregroundColorForTheme;

                var classificationType = e.ClassificationTypeRegistry.GetClassificationType(_classificationTypeName);

                if (classificationType == null)
                    return;

                ColorableItemInfo[] colorInfo = new ColorableItemInfo[1];

                if (e.FontAndColorStorage.GetItem(_classificationTypeName, colorInfo) != VSConstants.S_OK)    //comment from F# repo: "we don't touch the changes made by the user"
                {
                    var properties = e.FormatMap.GetTextProperties(classificationType);
                    var newProperties = properties.SetForeground(ForegroundColor.Value);

                    e.FormatMap.SetTextProperties(classificationType, newProperties);
                }
            }
			catch (Exception exception)
			{
                AcuminatorVSPackage.Instance?.AcuminatorLogger
                                            ?.LogException(exception, logOnlyFromAcuminatorAssemblies: false, Logger.LogMode.Error);
            }       
        }

        void IDisposable.Dispose()
        {
            if (AcuminatorVSPackage.Instance.ThemeUpdater != null)
            {
                AcuminatorVSPackage.Instance.ThemeUpdater.AcuminatorThemeChanged -= AcuminatorThemeChangedHandler;
            }
        }
    }
}
