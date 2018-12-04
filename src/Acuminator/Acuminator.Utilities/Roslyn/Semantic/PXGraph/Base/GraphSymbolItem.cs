using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	///  A non generic class for items of a graph with symbol
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class GraphNodeSymbolItem
	{
		/// <summary>
		/// Declaration symbol of the item of a graph
		/// </summary>
		public ISymbol SymbolBase { get; }

		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		public GraphNodeSymbolItem(ISymbol symbol, int declarationOrder)
		{
			symbol.ThrowOnNull(nameof(symbol));
			SymbolBase = symbol;
			DeclarationOrder = declarationOrder;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{SymbolBase.Name}";
	}
}
