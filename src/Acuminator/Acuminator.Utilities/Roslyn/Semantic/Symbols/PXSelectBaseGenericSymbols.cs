using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseGenericSymbols
    {
        private const string InsertMethodName = "Insert";
        private const string UpdateMethodName = "Update";
        private const string DeleteMethodName = "Delete";
        private const string SelectMethodName = "Select";

        public INamedTypeSymbol Type { get; }
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }
        public ImmutableArray<IMethodSymbol> Select { get; }

        internal PXSelectBaseGenericSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);
	        Insert = Type.GetMethods(InsertMethodName);
	        Update = Type.GetMethods(UpdateMethodName);
	        Delete = Type.GetMethods(DeleteMethodName);
            Select = Type.GetMembers()
                     .OfType<IMethodSymbol>()
                     .Where(m => m.Name.StartsWith(SelectMethodName, StringComparison.Ordinal))
                     .ToImmutableArray();
        }
    }
}
