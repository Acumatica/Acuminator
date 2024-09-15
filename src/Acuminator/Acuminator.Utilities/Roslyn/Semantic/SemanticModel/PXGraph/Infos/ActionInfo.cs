using System.Diagnostics;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// The DTO with information about the action declared in graph.
	/// </summary>
	public class ActionInfo : OverridableSymbolItem<ActionInfo, ISymbol>
	{
		/// <summary>
		/// Indicates whether the action is predefined system action in Acumatica like <see cref="PX.Data.PXSave{TNode}"/>
		/// </summary>
		public bool IsSystem { get; }

		/// <summary>
		/// The type of the action symbol.
		/// </summary>
		public INamedTypeSymbol Type { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected override string DebuggerDisplay => $"{base.DebuggerDisplay} |Type: {Type.ToString()}";

		public ActionInfo(ISymbol symbol, INamedTypeSymbol type, int declarationOrder, bool isSystem, ActionInfo baseInfo) :
					 this(symbol, type, declarationOrder, isSystem)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(_baseInfo);
		}

		public ActionInfo(ISymbol symbol, INamedTypeSymbol type, int declarationOrder, bool isSystem) :
					 base(symbol, declarationOrder)
		{
			Type = type.CheckIfNull();
			IsSystem = isSystem;
		}
	}
}
