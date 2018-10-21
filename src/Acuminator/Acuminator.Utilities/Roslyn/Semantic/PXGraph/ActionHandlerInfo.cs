using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the action's handler in graph.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class ActionHandlerInfo : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
    {
        /// <summary>
        /// The overriden handler if any
        /// </summary>
        public ActionHandlerInfo Base { get; }

        public ActionHandlerInfo(MethodDeclarationSyntax node, IMethodSymbol symbol) : base(node, symbol)
        {
        }

        public ActionHandlerInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, ActionHandlerInfo baseInfo) : this(node, symbol)
        {
            baseInfo.ThrowOnNull();
            Base = baseInfo;
        }
    }
}
