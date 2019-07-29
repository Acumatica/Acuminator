using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Generic class for items of a graph or DAC with symbol
	/// </summary>
	/// <typeparam name="T">Type of the declaration symbol of the item</typeparam>
	public abstract class SymbolItem<T> : SymbolItem
	where T : ISymbol
	{
		/// <summary>
		/// Declaration symbol of the item
		/// </summary>
		public T Symbol => (T)SymbolBase;

		public SymbolItem(T symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
