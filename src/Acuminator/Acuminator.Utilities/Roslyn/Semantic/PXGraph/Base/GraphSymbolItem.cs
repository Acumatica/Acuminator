using Microsoft.CodeAnalysis;
using PX.Common;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    /// <summary>
    /// Generic class for items of a graph with symbol
    /// </summary>
    /// <typeparam name="T">Type of the declaration symbol of the item</typeparam>
    public class GraphNodeSymbolItem<T>
        where T : ISymbol
    {
        /// <summary>
        /// Declaration symbol of the item of a graph
        /// </summary>
        public T Symbol { get; }

        public GraphNodeSymbolItem(T symbol)
        {
            symbol.ThrowOnNull();
            Symbol = symbol;
        }
    }
}
