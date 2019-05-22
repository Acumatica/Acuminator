using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;


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
            Type = compilation.GetTypeByMetadataName(Types.PXSelectBase1);
	        Insert = Type.GetMethods(Types.PXSelectBase1Delegates.Insert);
	        Update = Type.GetMethods(Types.PXSelectBase1Delegates.Update);
	        Delete = Type.GetMethods(Types.PXSelectBase1Delegates.Delete);
            Select = Type.GetMembers()
                     .OfType<IMethodSymbol>()
                     .Where(m => m.Name.StartsWith(Types.PXSelectBase1Delegates.Select, StringComparison.Ordinal))
                     .ToImmutableArray();
        }
    }
}
