using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseGenericSymbols
    {
        public INamedTypeSymbol Type { get; }
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }
        public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXSelectBaseGenericSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase1);
	        Insert = Type.GetMethods(DelegateNames.Insert);
	        Update = Type.GetMethods(DelegateNames.Update);
	        Delete = Type.GetMethods(DelegateNames.Delete);
            Select = Type.GetMembers()
                     .OfType<IMethodSymbol>()
                     .Where(m => m.Name.StartsWith(DelegateNames.Select, StringComparison.Ordinal))
                     .ToImmutableArray();
        }
    }
}
