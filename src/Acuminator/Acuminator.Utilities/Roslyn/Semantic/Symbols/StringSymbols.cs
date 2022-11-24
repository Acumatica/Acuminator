using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class StringSymbols : SymbolsSetForTypeBase
	{
		public ImmutableArray<ISymbol> StringFormat { get; }

		public ImmutableArray<ISymbol> StringConcat { get; }

		internal StringSymbols(Compilation compilation) :
						  base(compilation, compilation.GetSpecialType(SpecialType.System_String))
		{
			StringFormat = Type?.GetMembers(nameof(string.Format)) ?? ImmutableArray<ISymbol>.Empty;
			StringConcat = Type?.GetMembers(nameof(string.Concat)) ?? ImmutableArray<ISymbol>.Empty;
		}
	}
}
