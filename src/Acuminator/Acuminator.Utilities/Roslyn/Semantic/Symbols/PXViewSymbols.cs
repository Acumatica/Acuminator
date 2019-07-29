using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXViewSymbols : SymbolsSetForTypeBase
    {
	    public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXViewSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXView)
        {
	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(DelegateNames.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
        }
    }
}
