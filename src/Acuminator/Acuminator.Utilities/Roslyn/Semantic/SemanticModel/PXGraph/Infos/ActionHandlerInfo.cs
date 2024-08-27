using System;

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
		protected ActionHandlerInfo? _baseInfo;

		/// <summary>
		/// The overriden handler if any
		/// </summary>
		public ActionHandlerInfo? Base => _baseInfo;

		ActionHandlerInfo? IWriteableBaseItem<ActionHandlerInfo>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}


		public ActionHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder) :
							base(node, symbol, declarationOrder)
		{
		}

		public ActionHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder, ActionHandlerInfo baseInfo) :
							this(node, symbol, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(_baseInfo);
		}

		void IWriteableBaseItem<ActionHandlerInfo>.CombineWithBaseInfo(ActionHandlerInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		protected void CombineWithBaseInfo(ActionHandlerInfo baseInfo)
		{
		}
	}
}
