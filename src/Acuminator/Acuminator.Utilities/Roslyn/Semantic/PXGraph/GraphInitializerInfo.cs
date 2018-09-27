using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public readonly struct GraphInitializerInfo
    {
        public GraphInitializerType Type { get; }
        public CSharpSyntaxNode Node { get; }
        public ISymbol Symbol { get; }

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
