#nullable enable

using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

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
			Type.ThrowOnNull();

			GenericType = Compilation.GetTypeByMetadataName(TypeFullNames.PXCache1)!;

			Insert = Type.GetMethods(DelegateNames.Insert).ToImmutableArray();
			Update = Type.GetMethods(DelegateNames.Update).ToImmutableArray();
			Delete = Type.GetMethods(DelegateNames.Delete).ToImmutableArray();

			RaiseExceptionHandling   = Type.GetMethods(DelegateNames.RaiseExceptionHandling).ToImmutableArray();
		}
	}
}