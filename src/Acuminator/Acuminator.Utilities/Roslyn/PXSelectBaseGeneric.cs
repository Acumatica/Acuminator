using Microsoft.CodeAnalysis;
using PX.Data;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn
{
    public class PXSelectBaseGeneric
    {
        private const string InsertMethodName = "Insert";
        private const string UpdateMethodName = "Update";
        private const string DeleteMethodName = "Delete";

        public INamedTypeSymbol Type { get; }
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }

        internal PXSelectBaseGeneric(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);
            Insert = Type.GetMembers(InsertMethodName)
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
            Update = Type.GetMembers(UpdateMethodName)
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
            Delete = Type.GetMembers(DeleteMethodName)
                     .OfType<IMethodSymbol>()
                     .ToImmutableArray();
        }
    }
}
