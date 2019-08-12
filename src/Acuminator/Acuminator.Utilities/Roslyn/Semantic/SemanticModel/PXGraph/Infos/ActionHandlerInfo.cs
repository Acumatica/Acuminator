using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the action's handler in graph.
	/// </summary>
	public class ActionHandlerInfo : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>, IWriteableBaseItem<ActionHandlerInfo>
	{
		/// <summary>
		/// The overriden handler if any
		/// </summary>
		public ActionHandlerInfo Base
		{
			get;
			internal set;
		}

		ActionHandlerInfo IWriteableBaseItem<ActionHandlerInfo>.Base
		{
			get => Base;
			set => Base = value;
		}


		public ActionHandlerInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder) :
							base(node, symbol, declarationOrder)
		{
		}

		public ActionHandlerInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder, ActionHandlerInfo baseInfo) :
							this(node, symbol, declarationOrder)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;
		}
	}
}
