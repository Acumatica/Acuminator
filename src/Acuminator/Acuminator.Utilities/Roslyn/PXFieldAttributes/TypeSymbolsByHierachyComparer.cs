#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// A type symbols comparer that compares them by their position in class hierarhy and makes sure that derived types go before their base types. 
	/// Does not work for interfaces.
	/// </summary>
	internal class TypeSymbolsByHierachyComparer : IComparer<ITypeSymbol>
	{
		public static TypeSymbolsByHierachyComparer Instance { get; } = new TypeSymbolsByHierachyComparer();

		private TypeSymbolsByHierachyComparer()
		{ }

		/// <summary>
		/// Compares two type symbols objects to determine their relative ordering.<br/>
		/// If <paramref name="x"/> is derived from <paramref name="y"/> returns <c>-1</c> to make <paramref name="x"/> go before <paramref name="y"/>.<br/>
		/// If <paramref name="y"/> is derived from <paramref name="x"/> returns <c>1</c> to make <paramref name="x"/> go after <paramref name="y"/>.<br/>
		/// If <paramref name="x"/> and <paramref name="y"/> are not related returns <c>0</c>.<br/>
		/// </summary>
		/// <param name="x">First type symbol to be compared.</param>
		/// <param name="y">Second type symbol to be compared.</param>
		/// <returns>
		/// If <paramref name="x"/> is derived from <paramref name="y"/> returns <c>-1</c> to make <paramref name="x"/> go before <paramref name="y"/>.<br/>
		/// If <paramref name="y"/> is derived from <paramref name="x"/> returns <c>1</c> to make <paramref name="x"/> go after <paramref name="y"/>.<br/>
		/// If <paramref name="x"/> and <paramref name="y"/> are not related returns <c>0</c>.<br/>
		/// </returns>
		public int Compare(ITypeSymbol x, ITypeSymbol y)
		{
			if (x.Equals(y, SymbolEqualityComparer.Default))
				return 0;
			else if (x.InheritsFrom(y))
				return -1;
			else if (y.InheritsFrom(x))
				return 1;
			else
				return 0;
		}
	}
}
