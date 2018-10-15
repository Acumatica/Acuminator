using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class DataViewDelegateInfo
    {
        /// <summary>
        /// The overriden item if any
        /// </summary>
        public DataViewDelegateInfo Base { get; }

        /// <summary>
        /// The syntax node of the data view delegate declaration
        /// </summary>
        public MethodDeclarationSyntax Node { get; }

        /// <summary>
        /// The symbol of the data view delegate declaration
        /// </summary>
        public IMethodSymbol Symbol { get; }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            node.ThrowOnNull(nameof(node));
            symbol.ThrowOnNull(nameof(symbol));

            Node = node;
            Symbol = symbol;
        }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, DataViewDelegateInfo baseInfo)
            : this(node, symbol)
        {
            baseInfo.ThrowOnNull();

            Base = baseInfo;
        }
    }
}
