#nullable enable

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	///  A non generic class for items of a graph or DAC with symbol
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class SymbolItem
	{
		/// <summary>
		/// Declaration symbol of the item
		/// </summary>
		public ISymbol SymbolBase { get; }

		public virtual string Name => SymbolBase.Name;

		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		public SymbolItem(ISymbol symbol, int declarationOrder)
		{
			symbol.ThrowOnNull(nameof(symbol));
			SymbolBase = symbol;
			DeclarationOrder = declarationOrder;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";
	}
}
