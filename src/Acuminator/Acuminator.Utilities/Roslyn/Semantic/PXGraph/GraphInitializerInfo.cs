using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphInitializerInfo
    {
        public GraphInitializerType Type { get; private set; }
        public BaseMethodDeclarationSyntax Node { get; private set; }
        public IMethodSymbol Symbol { get; private set; }

        public GraphInitializerInfo(GraphInitializerType type, BaseMethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            node.ThrowOnNull(nameof(node));
            symbol.ThrowOnNull(nameof(symbol));

            Type = type;
            Node = node;
            Symbol = symbol;
        }
    }
}
