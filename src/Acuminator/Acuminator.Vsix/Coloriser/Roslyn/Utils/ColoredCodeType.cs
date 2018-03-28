using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Acuminator.Analyzers.Coloriser
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
