using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Generic class for items of a graph with symbol
	/// </summary>
	/// <typeparam name="T">Type of the declaration symbol of the item</typeparam>
	public abstract class GraphNodeSymbolItem<T> : GraphNodeSymbolItem
	where T : ISymbol
	{
		/// <summary>
		/// Declaration symbol of the item of a graph
		/// </summary>
		public T Symbol => (T)SymbolBase;

		public GraphNodeSymbolItem(T symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
