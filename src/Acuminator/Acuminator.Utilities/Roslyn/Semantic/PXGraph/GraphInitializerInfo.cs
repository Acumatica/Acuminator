using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphInitializerInfo : GraphNodeSymbolItem<SyntaxNode, ISymbol>
    {
        public GraphInitializerType Type { get; }

        public GraphInitializerInfo(GraphInitializerType type, SyntaxNode node, ISymbol symbol)
            : base(node, symbol)
        {
            Type = type;
        }
    }
}
