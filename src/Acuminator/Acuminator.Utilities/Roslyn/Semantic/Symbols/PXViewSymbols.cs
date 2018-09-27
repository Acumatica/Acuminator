using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXViewSymbols
    {
        public INamedTypeSymbol Type { get; }

	    public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXViewSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXView).FullName);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(nameof(PX.Data.PXView.Select), StringComparison.Ordinal))
		        .ToImmutableArray();
        }
    }
}
