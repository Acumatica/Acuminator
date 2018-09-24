using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXCacheSymbols
    {
        public INamedTypeSymbol Type { get; }
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }

        internal PXCacheSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXCache).FullName);
	        Insert = Type.GetMethods(nameof(PX.Data.PXCache.Insert));
	        Update = Type.GetMethods(nameof(PX.Data.PXCache.Update));
	        Delete = Type.GetMethods(nameof(PX.Data.PXCache.Delete));
        }
    }
}
