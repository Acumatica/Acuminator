using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Constants;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectorAttributeSymbols : SymbolsSetForTypeBase
    {
	    public ImmutableArray<IMethodSymbol> Select { get; }
		public ImmutableArray<IMethodSymbol> GetItem { get; }

        internal PXSelectorAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXSelectorAttribute)
        {
	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(DelegateNames.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
	        GetItem = Type.GetMethods(DelegateNames.GetItem);
        }
    }
}
