using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Coloriser
{
    internal class AcuminatorThemeChangedEventArgs : EventArgs
    {
        public IVsFontAndColorStorage FontAndColorStorage { get; }

        public IClassificationTypeRegistryService ClassificationTypeRegistry { get; }

        public IClassificationFormatMap FormatMap { get; }

        public AcuminatorThemeChangedEventArgs(IVsFontAndColorStorage fontAndColorStorage, IClassificationTypeRegistryService classificationTypeRegistry,
                                               IClassificationFormatMap formatMap)
		{
            FontAndColorStorage = fontAndColorStorage.CheckIfNull(nameof(fontAndColorStorage));
            ClassificationTypeRegistry = classificationTypeRegistry.CheckIfNull(nameof(classificationTypeRegistry));
            FormatMap = formatMap;
        }
    }
}
