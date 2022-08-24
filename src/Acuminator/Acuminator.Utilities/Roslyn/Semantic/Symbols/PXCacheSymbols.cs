#nullable enable

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

		public INamedTypeSymbol GenericType { get; }

		internal PXCacheSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXCache)
		{
			GenericType = Compilation.GetTypeByMetadataName(TypeFullNames.PXCache1);

			Insert = Type.GetMethods(DelegateNames.Insert);
			Update = Type.GetMethods(DelegateNames.Update);
			Delete = Type.GetMethods(DelegateNames.Delete);

			RaiseExceptionHandling = Type.GetMethods(DelegateNames.RaiseExceptionHandling);
		}
	}
}