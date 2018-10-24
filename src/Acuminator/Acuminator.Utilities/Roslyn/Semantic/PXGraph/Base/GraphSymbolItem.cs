using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Generic class for items of a graph with symbol
	/// </summary>
	/// <typeparam name="T">Type of the declaration symbol of the item</typeparam>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class GraphNodeSymbolItem<T>
        where T : ISymbol
    {
		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		/// <summary>
		/// Declaration symbol of the item of a graph
		/// </summary>
		public T Symbol { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Symbol.Name}";

		public GraphNodeSymbolItem(T symbol, int declarationOrder)
        {
            symbol.ThrowOnNull(nameof(symbol));
            Symbol = symbol;
			DeclarationOrder = declarationOrder;
        }
    }
}
