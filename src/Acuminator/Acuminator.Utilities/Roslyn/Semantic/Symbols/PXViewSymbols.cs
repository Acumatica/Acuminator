using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXViewSymbols
    {
        public INamedTypeSymbol Type { get; }

	    public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXViewSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXView);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(Types.PXViewDelegates.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
        }
    }
}
