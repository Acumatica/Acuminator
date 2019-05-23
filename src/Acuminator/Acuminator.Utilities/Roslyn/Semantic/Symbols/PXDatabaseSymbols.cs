using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXDatabaseSymbols
    {
        public INamedTypeSymbol Type { get; }

	    public ImmutableArray<IMethodSymbol> Select { get; }
		public ImmutableArray<IMethodSymbol> Insert { get; }
	    public ImmutableArray<IMethodSymbol> Update { get; }
	    public ImmutableArray<IMethodSymbol> Delete { get; }
	    public ImmutableArray<IMethodSymbol> Ensure { get; }

        internal PXDatabaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXDatabase);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(Types.PXDatabaseDelegates.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
	        Insert = Type.GetMethods(Types.PXDatabaseDelegates.Insert);
	        Update = Type.GetMethods(Types.PXDatabaseDelegates.Update);
	        Delete = Type.GetMethods(Types.PXDatabaseDelegates.Delete)
		        .Concat(Type.GetMethods(Types.PXDatabaseDelegates.ForceDelete))
		        .ToImmutableArray();
	        Ensure = Type.GetMethods(Types.PXDatabaseDelegates.Ensure);
        }
    }
}
