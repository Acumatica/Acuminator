using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseGenericSymbols : SymbolsSetForTypeBase
    {
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }
        public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXSelectBaseGenericSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXSelectBase1)
        {
	        Insert = Type.GetMethods(DelegateNames.Insert).ToImmutableArray();
	        Update = Type.GetMethods(DelegateNames.Update).ToImmutableArray();
	        Delete = Type.GetMethods(DelegateNames.Delete).ToImmutableArray();
            Select = Type.GetMethods()
						 .Where(m => m.Name.StartsWith(DelegateNames.Select, StringComparison.Ordinal))
						 .ToImmutableArray();
        }
    }
}
