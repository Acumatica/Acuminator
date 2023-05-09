using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public sealed class SymbolEqualityComparer : IEqualityComparer<ISymbol>
	{
		public static readonly SymbolEqualityComparer Instance = new();

		public bool Equals(ISymbol x, ISymbol y)
		{
			if (x == null)
			{
				return y == null;
			}

			return x.Equals(y);
		}

		public int GetHashCode(ISymbol obj) => obj?.GetHashCode() ?? 0;
	}
}
