using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public readonly struct DataViewDelegateInfo
    {
        public MethodDeclarationSyntax Node { get; }
        public IMethodSymbol Symbol { get; }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            Node = node;
            Symbol = symbol;
        }
    }
}
