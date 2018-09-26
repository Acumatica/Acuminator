using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public readonly struct StaticConstructorInfo
    {
        public ConstructorDeclarationSyntax Node { get; }
        public IMethodSymbol Symbol { get; }

        public StaticConstructorInfo(ConstructorDeclarationSyntax node, IMethodSymbol symbol)
        {
            node.ThrowOnNull(nameof(node));
            symbol.ThrowOnNull(nameof(symbol));

            Node = node;
            Symbol = symbol;
        }
    }
}
