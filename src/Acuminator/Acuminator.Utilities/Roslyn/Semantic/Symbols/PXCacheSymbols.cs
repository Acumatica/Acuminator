#nullable enable

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;
using System;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXCacheSymbols : SymbolsSetForTypeBase
	{
		public ImmutableArray<IMethodSymbol> Insert { get; }
		public ImmutableArray<IMethodSymbol> Update { get; }
		public ImmutableArray<IMethodSymbol> Delete { get; }

		public ImmutableArray<IMethodSymbol> RaiseExceptionHandling { get; }

		public INamedTypeSymbol GenericType { get; }
		
		public IEventSymbol? RowSelectingWhileReading { get; }

		internal PXCacheSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXCache)
		{
			Type.ThrowOnNull(nameof(Type));

			GenericType = Compilation.GetTypeByMetadataName(TypeFullNames.PXCache1);

			Insert = Type.GetMethods(DelegateNames.Insert);
			Update = Type.GetMethods(DelegateNames.Update);
			Delete = Type.GetMethods(DelegateNames.Delete);

			RaiseExceptionHandling = Type.GetMethods(DelegateNames.RaiseExceptionHandling);
			RowSelectingWhileReading = Type.GetMembers(EventsNames.PXCache.RowSelectingWhileReading)
										   .OfType<IEventSymbol>()
										   .FirstOrDefault();
		}
	}
}