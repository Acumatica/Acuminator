using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectExtensionSymbols : SymbolsSetForTypeBase
	{      
        internal PXSelectExtensionSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXSelectExtension1)
        {
        }
    }
}
