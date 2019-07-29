using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXDatabaseSymbols : SymbolsSetForTypeBase
    {
	    public ImmutableArray<IMethodSymbol> Select { get; }
		public ImmutableArray<IMethodSymbol> Insert { get; }
	    public ImmutableArray<IMethodSymbol> Update { get; }
	    public ImmutableArray<IMethodSymbol> Delete { get; }
	    public ImmutableArray<IMethodSymbol> Ensure { get; }

        internal PXDatabaseSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXDatabase)
        {
	        Select = Type.GetMethods()
		        .Where(m => m.Name.StartsWith(DelegateNames.Select, StringComparison.Ordinal))
		        .ToImmutableArray();
	        Insert = Type.GetMethods(DelegateNames.Insert);
	        Update = Type.GetMethods(DelegateNames.Update);
	        Delete = Type.GetMethods(DelegateNames.Delete)
		        .Concat(Type.GetMethods(DelegateNames.ForceDelete))
		        .ToImmutableArray();
	        Ensure = Type.GetMethods(DelegateNames.Ensure);
        }
    }
}
