using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants.Types;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectorAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

	    public ImmutableArray<IMethodSymbol> Select { get; }
		public ImmutableArray<IMethodSymbol> GetItem { get; }

        internal PXSelectorAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(PXSelectorAttribute);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(PXSelectorAttributeDelegates.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
	        GetItem = Type.GetMethods(PXSelectorAttributeDelegates.GetItem);
        }
    }
}
