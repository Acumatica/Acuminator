using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace PX.Analyzers
{
	internal static class SymbolHelpers
	{
		public static IEnumerable<ITypeSymbol> GetBaseTypes(this ITypeSymbol type)
		{
			var current = type.BaseType;
			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}

		public static bool InheritsFrom(
			this ITypeSymbol type, ITypeSymbol baseType)
		{
			return type.GetBaseTypes().Any(t => t.Equals(baseType));
		}

		public static bool InheritsFrom(this ITypeSymbol symbol, string baseType)
		{
			var current = symbol;
			while (current != null)
			{
				if (current.Name == baseType) return true;
				current = current.BaseType;
			}
			return false;
		}
	}
}
