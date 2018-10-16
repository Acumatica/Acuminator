using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class DataViewDelegateInfo
    {
        public MethodDeclarationSyntax Node { get; }
        public IMethodSymbol Symbol { get; }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            node.ThrowOnNull(nameof(node));
            symbol.ThrowOnNull(nameof(symbol));

            Node = node;
            Symbol = symbol;
        }
    }
}
