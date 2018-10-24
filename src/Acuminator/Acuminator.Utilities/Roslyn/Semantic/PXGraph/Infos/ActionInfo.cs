using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// The DTO with information about the action declared in graph.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class ActionInfo : GraphNodeSymbolItem<ISymbol>
    {
		/// <summary>
		/// The overriden action if any
		/// </summary>
		public ActionInfo Base { get; }

		/// <summary>
		/// Indicates whether the action is predefined system action in Acumatica like <see cref="PX.Data.PXSave{TNode}"/>
		/// </summary>
		public bool IsSystem { get; }

        /// <summary>
        /// The type of the action symbol.
        /// </summary>
        public INamedTypeSymbol Type { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => $"Symbol: {Symbol.Name} | Type: {Type.ToString()}";

		public ActionInfo(ISymbol symbol, INamedTypeSymbol type, int declarationOrder, bool isSystem) : 
					 base(symbol, declarationOrder)
        {
            type.ThrowOnNull(nameof(type));

            Type = type;
			IsSystem = isSystem;
        }

		public ActionInfo(ISymbol symbol, INamedTypeSymbol type, int declarationOrder, bool isSystem, ActionInfo baseInfo) : 
					 this(symbol, type, declarationOrder, isSystem)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}
	}
}
