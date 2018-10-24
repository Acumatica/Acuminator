using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    /// <summary>
    /// Generic class for items of a graph with node and symbol
    /// </summary>
    /// <typeparam name="N">Type of the declaration node of the item</typeparam>
    /// <typeparam name="S">Type of the declaration symbol of the item</typeparam>
    public class GraphNodeSymbolItem<N, S> : GraphNodeSymbolItem<S>
        where N : SyntaxNode
        where S : ISymbol
    {
        /// <summary>
        /// The declaration node of the item of a graph
        /// </summary>
        public N Node { get; }

        public GraphNodeSymbolItem(N node, S symbol, int declarationOrder)
            : base(symbol, declarationOrder)
        {
            node.ThrowOnNull(nameof(node));
            Node = node;
        }
    }
}
