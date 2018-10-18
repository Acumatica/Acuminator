using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class DataViewDelegateInfo : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
    {
        /// <summary>
        /// The overriden item if any
        /// </summary>
        public DataViewDelegateInfo Base { get; }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol)
            : base(node, symbol)
        {
        }

        public DataViewDelegateInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, DataViewDelegateInfo baseInfo)
            : this(node, symbol)
        {
            baseInfo.ThrowOnNull();
            Base = baseInfo;
        }
    }
}
