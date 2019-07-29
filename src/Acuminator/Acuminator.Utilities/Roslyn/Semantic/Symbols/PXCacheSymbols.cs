using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXCacheSymbols : SymbolsSetForTypeBase
    {
        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }

	    public ImmutableArray<IMethodSymbol> RaiseExceptionHandling { get; }

        internal PXCacheSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXCache)
        {
	        Insert = Type.GetMethods(DelegateNames.Insert);
	        Update = Type.GetMethods(DelegateNames.Update);
	        Delete = Type.GetMethods(DelegateNames.Delete);

	        RaiseExceptionHandling = Type.GetMethods(DelegateNames.RaiseExceptionHandling);
        }
    }
}
