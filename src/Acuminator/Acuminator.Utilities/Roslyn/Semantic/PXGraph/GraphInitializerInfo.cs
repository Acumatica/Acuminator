using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphInitializerInfo
    {
        public GraphInitializerType Type { get; private set; }
        public CSharpSyntaxNode Node { get; private set; }
        public ISymbol Symbol { get; private set; }

        public GraphInitializerInfo(GraphInitializerType type, CSharpSyntaxNode node, ISymbol symbol)
        {
            node.ThrowOnNull(nameof(node));
            symbol.ThrowOnNull(nameof(symbol));

            Type = type;
            Node = node;
            Symbol = symbol;
        }
    }
}
