using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Acuminator.Utilities
{
    /// <summary>
    /// Values that represent classification types for the coloring of code fragments.
    /// </summary>
    public enum ColoredCodeType 
    {
        BqlCommand,
        BqlOperator,
        Dac,
        DacField,
        DacExtension,
        BqlParameter,
        BQLConstantPrefix,
        BQLConstantEnding,
        PXGraph,
        PXAction
    }
}
