using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers
{
    /// <summary>
    /// A simple wrapper for the Acuminator settings.
    /// </summary>
    public static class AcuminatorSettings
    {
        /// <summary>
        /// A value indicating whether to use Regular Expression or Roslyn syntax coloring.
        /// </summary>
        public static bool UseRegexColoriser
        {
            get => PXAnalysersSettings.Default.UseRegexColoriser;
            set
            {
                if (PXAnalysersSettings.Default.UseRegexColoriser != value)
                {
                    PXAnalysersSettings.Default.UseRegexColoriser = value;
                    PXAnalysersSettings.Default.Save();
                }
            }
        }

        /// <summary>
        /// A value indicating whether the syntax coloring is enabled.
        /// </summary>
        public static bool ColoringEnabled
        {
            get => PXAnalysersSettings.Default.ColoringEnabled;
            set
            {
                if (PXAnalysersSettings.Default.ColoringEnabled != value)
                {
                    PXAnalysersSettings.Default.ColoringEnabled = value;
                    PXAnalysersSettings.Default.Save();
                }
            }
        }
    }
}
