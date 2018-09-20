using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn
{
    public class PXCache
    {
        public INamedTypeSymbol Type { get; }
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }

        public PXCache(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXCache).FullName);
            Insert = Type.GetMembers(nameof(PX.Data.PXCache.Insert))
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
            Update = Type.GetMembers(nameof(PX.Data.PXCache.Update))
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
            Delete = Type.GetMembers(nameof(PX.Data.PXCache.Delete))
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
        }
    }
}
