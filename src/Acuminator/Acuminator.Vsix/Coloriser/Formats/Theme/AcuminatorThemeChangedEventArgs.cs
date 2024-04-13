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
    internal class AcuminatorThemeChangedEventArgs(IVsFontAndColorStorage fontAndColorStorage, 
												   IClassificationTypeRegistryService classificationTypeRegistry,
												   IClassificationFormatMap formatMap) : EventArgs
    {
		public IVsFontAndColorStorage FontAndColorStorage { get; } = fontAndColorStorage.CheckIfNull();

		public IClassificationTypeRegistryService ClassificationTypeRegistry { get; } = classificationTypeRegistry.CheckIfNull();

		public IClassificationFormatMap FormatMap { get; } = formatMap;
	}
}
