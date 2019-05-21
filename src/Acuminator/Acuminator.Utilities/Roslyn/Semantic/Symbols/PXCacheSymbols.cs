using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXCacheSymbols
    {
        public INamedTypeSymbol Type { get; }

        public ImmutableArray<IMethodSymbol> Insert { get; }
        public ImmutableArray<IMethodSymbol> Update { get; }
        public ImmutableArray<IMethodSymbol> Delete { get; }

	    public ImmutableArray<IMethodSymbol> RaiseExceptionHandling { get; }

        internal PXCacheSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXCache);

	        Insert = Type.GetMethods(Types.PXCacheNames.Insert);
	        Update = Type.GetMethods(Types.PXCacheNames.Update);
	        Delete = Type.GetMethods(Types.PXCacheNames.Delete);

	        RaiseExceptionHandling = Type.GetMethods(Types.PXCacheNames.RaiseExceptionHandling));
        }
    }
}
