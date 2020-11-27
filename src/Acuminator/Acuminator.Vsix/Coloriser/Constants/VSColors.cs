using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace Acuminator.Vsix.Coloriser
{
    [SuppressMessage("Style",
        "VSTHRD010: Accessing \"Acuminator.Vsix.Coloriser.VSColors.IsDarkTheme\" should only be done on the main thread.Call Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread() first.",
        Justification = "Already called inside", Scope = "type")]
    public static class VSColors
    {
        private const int NOT_INITIALIZED = 0, INITIALIZED = 1;

        private const byte RedCriteria = 128;
        private const byte GreenCriteria = 128;
        private const byte BlueCriteria = 128;

        private static IVsUIShell5 _vsUIShell5;
        private static int _serviceInitialized = NOT_INITIALIZED;

        private static readonly Dictionary<string, Func<Color>> _acuminatorColors = new Dictionary<string, Func<Color>>()
        {
            { ColoringConstants.BQLOperatorFormat, () => BQLOperatorFormatColor },
            { ColoringConstants.BQLParameterFormat, () => BQLParameterFormatColor },
            { ColoringConstants.DacFormat, () => DacFormatColor },
            { ColoringConstants.DacExtensionFormat, () => DacExtensionFormatColor },
            { ColoringConstants.DacFieldFormat, () => DacFieldFormatColor },
            { ColoringConstants.BQLConstantEndingFormat, () => BQLConstantEndingFormatColor },
            { ColoringConstants.BQLConstantPrefixFormat, () => BQLConstantPrefixFormatColor },

            { ColoringConstants.PXActionFormat, () => PXActionFormatColor },
            { ColoringConstants.PXGraphFormat, () => PXGraphFormatColor },

            { ColoringConstants.BraceLevel_1_Format, () => BraceLevel_1_FormatColor },
            { ColoringConstants.BraceLevel_2_Format, () => BraceLevel_2_FormatColor },
            { ColoringConstants.BraceLevel_3_Format, () => BraceLevel_3_FormatColor },
            { ColoringConstants.BraceLevel_4_Format, () => BraceLevel_4_FormatColor },
            { ColoringConstants.BraceLevel_5_Format, () => BraceLevel_5_FormatColor },
            { ColoringConstants.BraceLevel_6_Format, () => BraceLevel_6_FormatColor },
            { ColoringConstants.BraceLevel_7_Format, () => BraceLevel_7_FormatColor },
            { ColoringConstants.BraceLevel_8_Format, () => BraceLevel_8_FormatColor },
            { ColoringConstants.BraceLevel_9_Format, () => BraceLevel_9_FormatColor },
            { ColoringConstants.BraceLevel_10_Format, () => BraceLevel_10_FormatColor },
            { ColoringConstants.BraceLevel_11_Format, () => BraceLevel_11_FormatColor },
            { ColoringConstants.BraceLevel_12_Format, () => BraceLevel_12_FormatColor },
            { ColoringConstants.BraceLevel_13_Format, () => BraceLevel_13_FormatColor },
            { ColoringConstants.BraceLevel_14_Format, () => BraceLevel_14_FormatColor }
        };

        public static Color? GetThemedColor(string formatName)
        {
            if (formatName.IsNullOrWhiteSpace())
                return null;

            return _acuminatorColors.TryGetValue(formatName, out Func<Color> colorGetter)
                ? colorGetter()
                : (Color?)null;
        }

        public static bool IsDarkTheme()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Interlocked.Exchange(ref _serviceInitialized, INITIALIZED) == NOT_INITIALIZED)
            {
                _vsUIShell5 = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell5;
            }

            if (_vsUIShell5 == null)
                return true;

            Color editorBackgroundColor = _vsUIShell5.GetThemedWPFColor(EnvironmentColors.DarkColorKey);

            if (editorBackgroundColor == null)
                return true;

            return editorBackgroundColor.R < RedCriteria ||
                   editorBackgroundColor.G < GreenCriteria ||
                   editorBackgroundColor.B < BlueCriteria;
        }

        #region Colors    
        #region BQLOperator
        public static Color BQLOperatorFormatColorDark => Color.FromRgb(r: 181, g: 121, b: 255);
        public static Color BQLOperatorFormatColorLight => Color.FromRgb(r: 111, g: 66, b: 193);

        public static Color BQLOperatorFormatColor => IsDarkTheme()
            ? BQLOperatorFormatColorDark
            : BQLOperatorFormatColorLight;
        #endregion

        #region BQLParameter
        public static Color BQLParameterFormatColorDark => Color.FromRgb(r: 255, g: 79, b: 255);
        public static Color BQLParameterFormatColorLight => Color.FromRgb(r: 240, g: 18, b: 190);

        public static Color BQLParameterFormatColor => IsDarkTheme()
            ? BQLParameterFormatColorDark
            : BQLParameterFormatColorLight;
        #endregion

        #region Dac
        public static Color DacFormatColorDark => Color.FromRgb(r: 255, g: 153, b: 0);
        public static Color DacFormatColorLight => Color.FromRgb(r: 253, g: 126, b: 20);

        public static Color DacFormatColor => IsDarkTheme()
            ? DacFormatColorDark
            : DacFormatColorLight;
        #endregion

        #region DacExtension
        public static Color DacExtensionFormatColorDark => Color.FromRgb(r: 202, g: 153, b: 102);
        public static Color DacExtensionFormatColorLight => Color.FromRgb(r: 202, g: 153, b: 102);

        public static Color DacExtensionFormatColor => IsDarkTheme()
            ? DacExtensionFormatColorDark
            : DacExtensionFormatColorLight;
        #endregion

        #region DacField
        public static Color DacFieldFormatColorDark => Color.FromRgb(r: 76, g: 255, b: 79);
        public static Color DacFieldFormatColorLight => Color.FromRgb(r: 40, g: 167, b: 69);

        public static Color DacFieldFormatColor => IsDarkTheme()
            ? DacFieldFormatColorDark
            : DacFieldFormatColorLight;
        #endregion

        #region BQLConstantEnding
        public static Color BQLConstantEndingFormatColorDark => Color.FromRgb(r: 1, g: 153, b: 153);
        public static Color BQLConstantEndingFormatColorLight => Color.FromRgb(r: 1, g: 153, b: 153);

        public static Color BQLConstantEndingFormatColor => IsDarkTheme()
            ? BQLConstantEndingFormatColorDark
            : BQLConstantEndingFormatColorLight;
        #endregion

        #region BQLConstantPrefix
        public static Color BQLConstantPrefixFormatColorDark => Color.FromRgb(r: 255, g: 153, b: 153);
        public static Color BQLConstantPrefixFormatColorLight => Color.FromRgb(r: 241, g: 121, b: 124);

        public static Color BQLConstantPrefixFormatColor => IsDarkTheme()
            ? BQLConstantPrefixFormatColorDark
            : BQLConstantPrefixFormatColorLight;
        #endregion

        #region PXAction
        public static Color PXActionFormatColorDark => Color.FromRgb(r: 187, g: 55, b: 167);
        public static Color PXActionFormatColorLight => Color.FromRgb(r: 187, g: 55, b: 167);

        public static Color PXActionFormatColor => IsDarkTheme()
            ? PXActionFormatColorDark
            : PXActionFormatColorLight;
        #endregion

        #region PXGraph
        public static Color PXGraphFormatColorDark => Color.FromRgb(r: 38, g: 155, b: 199);
        public static Color PXGraphFormatColorLight => Color.FromRgb(r: 38, g: 155, b: 199);

        public static Color PXGraphFormatColor => IsDarkTheme()
            ? PXGraphFormatColorDark
            : PXGraphFormatColorLight;
        #endregion

        #region BraceLevel_1_
        public static Color BraceLevel_1_FormatColorDark => Color.FromRgb(r: 255, g: 255, b: 255);
        public static Color BraceLevel_1_FormatColorLight => Color.FromRgb(r: 54, g: 58, b: 65);

        public static Color BraceLevel_1_FormatColor => IsDarkTheme()
            ? BraceLevel_1_FormatColorDark
            : BraceLevel_1_FormatColorLight;
        #endregion

        #region BraceLevel_2_
        public static Color BraceLevel_2_FormatColorDark => Color.FromRgb(r: 172, g: 165, b: 133);
        public static Color BraceLevel_2_FormatColorLight => Color.FromRgb(r: 124, g: 124, b: 124);

        public static Color BraceLevel_2_FormatColor => IsDarkTheme()
            ? BraceLevel_2_FormatColorDark
            : BraceLevel_2_FormatColorLight;
        #endregion

        #region BraceLevel_3_
        public static Color BraceLevel_3_FormatColorDark => Color.FromRgb(r: 255, g: 255, b: 78);
        public static Color BraceLevel_3_FormatColorLight => Color.FromRgb(r: 255, g: 193, b: 9);

        public static Color BraceLevel_3_FormatColor => IsDarkTheme()
            ? BraceLevel_3_FormatColorDark
            : BraceLevel_3_FormatColorLight;
        #endregion

        #region BraceLevel_4_
        public static Color BraceLevel_4_FormatColorDark => Color.FromRgb(r: 255, g: 78, b: 79);
        public static Color BraceLevel_4_FormatColorLight => Color.FromRgb(r: 220, g: 53, b: 69);

        public static Color BraceLevel_4_FormatColor => IsDarkTheme()
            ? BraceLevel_4_FormatColorDark
            : BraceLevel_4_FormatColorLight;
        #endregion

        #region BraceLevel_5_
        public static Color BraceLevel_5_FormatColorDark => Color.FromRgb(r: 78, g: 255, b: 255);
        public static Color BraceLevel_5_FormatColorLight => Color.FromRgb(r: 60, g: 215, b: 215);

        public static Color BraceLevel_5_FormatColor => IsDarkTheme()
            ? BraceLevel_5_FormatColorDark
            : BraceLevel_5_FormatColorLight;
        #endregion

        #region BraceLevel_6_
        public static Color BraceLevel_6_FormatColorDark => Color.FromRgb(r: 255, g: 79, b: 255);
        public static Color BraceLevel_6_FormatColorLight => Color.FromRgb(r: 240, g: 18, b: 190);

        public static Color BraceLevel_6_FormatColor => IsDarkTheme()
            ? BraceLevel_6_FormatColorDark
            : BraceLevel_6_FormatColorLight;
        #endregion

        #region BraceLevel_7_
        public static Color BraceLevel_7_FormatColorDark => Color.FromRgb(r: 126, g: 211, b: 33);
        public static Color BraceLevel_7_FormatColorLight => Color.FromRgb(r: 40, g: 167, b: 69);

        public static Color BraceLevel_7_FormatColor => IsDarkTheme()
            ? BraceLevel_7_FormatColorDark
            : BraceLevel_7_FormatColorLight;
        #endregion

        #region BraceLevel_8_
        public static Color BraceLevel_8_FormatColorDark => Color.FromRgb(r: 204, g: 153, b: 101);
        public static Color BraceLevel_8_FormatColorLight => Color.FromRgb(r: 204, g: 153, b: 101);

        public static Color BraceLevel_8_FormatColor => IsDarkTheme()
            ? BraceLevel_8_FormatColorDark
            : BraceLevel_8_FormatColorLight;
        #endregion

        #region BraceLevel_9_
        public static Color BraceLevel_9_FormatColorDark => Color.FromRgb(r: 153, g: 51, b: 255);
        public static Color BraceLevel_9_FormatColorLight => Color.FromRgb(r: 111, g: 66, b: 193);

        public static Color BraceLevel_9_FormatColor => IsDarkTheme()
            ? BraceLevel_9_FormatColorDark
            : BraceLevel_9_FormatColorLight;
        #endregion

        #region BraceLevel_10_
        public static Color BraceLevel_10_FormatColorDark => Color.FromRgb(r: 255, g: 153, b: 0);
        public static Color BraceLevel_10_FormatColorLight => Color.FromRgb(r: 253, g: 126, b: 20);

        public static Color BraceLevel_10_FormatColor => IsDarkTheme()
            ? BraceLevel_10_FormatColorDark
            : BraceLevel_10_FormatColorLight;
        #endregion

        #region BraceLevel_11_
        public static Color BraceLevel_11_FormatColorDark => Color.FromRgb(r: 1, g: 153, b: 153);
        public static Color BraceLevel_11_FormatColorLight => Color.FromRgb(r: 23, g: 162, b: 184);

        public static Color BraceLevel_11_FormatColor => IsDarkTheme()
            ? BraceLevel_11_FormatColorDark
            : BraceLevel_11_FormatColorLight;
        #endregion

        #region BraceLevel_12_
        public static Color BraceLevel_12_FormatColorDark => Color.FromRgb(r: 255, g: 153, b: 153);
        public static Color BraceLevel_12_FormatColorLight => Color.FromRgb(r: 255, g: 153, b: 153);

        public static Color BraceLevel_12_FormatColor => IsDarkTheme()
            ? BraceLevel_12_FormatColorDark
            : BraceLevel_12_FormatColorLight;
        #endregion

        #region BraceLevel_13_
        public static Color BraceLevel_13_FormatColorDark => Color.FromRgb(r: 172, g: 195, b: 159);
        public static Color BraceLevel_13_FormatColorLight => Color.FromRgb(r: 172, g: 195, b: 159);

        public static Color BraceLevel_13_FormatColor => IsDarkTheme()
            ? BraceLevel_13_FormatColorDark
            : BraceLevel_13_FormatColorLight;
        #endregion

        #region BraceLevel_14_
        public static Color BraceLevel_14_FormatColorDark => Color.FromRgb(r: 153, g: 0, b: 0);
        public static Color BraceLevel_14_FormatColorLight => Color.FromRgb(r: 153, g: 0, b: 0);

        public static Color BraceLevel_14_FormatColor => IsDarkTheme()
            ? BraceLevel_14_FormatColorDark
            : BraceLevel_14_FormatColorLight;
        #endregion
        #endregion      
    }
}
