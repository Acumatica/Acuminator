using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

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
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXDatabase).FullName);

	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(nameof(PX.Data.PXDatabase.Select), StringComparison.Ordinal))
		        .ToImmutableArray();
	        Insert = Type.GetMethods(nameof(PX.Data.PXDatabase.Insert));
	        Update = Type.GetMethods(nameof(PX.Data.PXDatabase.Update));
	        Delete = Type.GetMethods(nameof(PX.Data.PXDatabase.Delete))
		        .Concat(Type.GetMethods(nameof(PX.Data.PXDatabase.ForceDelete)))
		        .ToImmutableArray();
	        Ensure = Type.GetMethods(nameof(PX.Data.PXDatabase.Ensure));
        }
    }
}
