using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class InitDelegateInfo : GraphNodeSymbolItem<SyntaxNode, ISymbol>
    {
        public GraphType GraphType => GraphType.PXGraph;
        public INamedTypeSymbol GraphTypeSymbol { get; }

        public InitDelegateInfo(INamedTypeSymbol graphSymbol, ISymbol delegateSymbol, SyntaxNode delegateNode)
            : base(delegateNode, delegateSymbol)
        {
            graphSymbol.ThrowOnNull(nameof(graphSymbol));
            GraphTypeSymbol = graphSymbol;
        }
    }
}
