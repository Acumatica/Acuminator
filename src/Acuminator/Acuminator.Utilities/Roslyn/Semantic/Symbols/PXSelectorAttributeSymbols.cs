using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectorAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

	    public ImmutableArray<IMethodSymbol> Select { get; }
		public ImmutableArray<IMethodSymbol> GetItem { get; }

        internal PXSelectorAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXSelectorAttribute).FullName);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(nameof(PX.Data.PXSelectorAttribute.Select), StringComparison.Ordinal))
		        .ToImmutableArray();
	        GetItem = Type.GetMethods(nameof(PX.Data.PXSelectorAttribute.GetItem));
        }
    }
}
