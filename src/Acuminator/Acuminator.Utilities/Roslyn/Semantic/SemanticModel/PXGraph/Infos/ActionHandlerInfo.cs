using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the action's handler in graph.
	/// </summary>
	public class ActionHandlerInfo : OverridableNodeSymbolItem<ActionHandlerInfo, MethodDeclarationSyntax, IMethodSymbol>
	{
		public ActionHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder) :
							base(node, symbol, declarationOrder)
		{
		}

		public ActionHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder, ActionHandlerInfo baseInfo) :
							base(node, symbol, declarationOrder, baseInfo)
		{
		}
	}
}
