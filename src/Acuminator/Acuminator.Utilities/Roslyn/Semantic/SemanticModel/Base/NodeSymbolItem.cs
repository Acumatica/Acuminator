#nullable enable

using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Generic class for items of a graph or DAC with node and symbol
	/// </summary>
	/// <typeparam name="N">Type of the declaration node of the item</typeparam>
	/// <typeparam name="S">Type of the declaration symbol of the item</typeparam>
	public abstract class NodeSymbolItem<N, S> : SymbolItem<S>
		where N : SyntaxNode
		where S : ISymbol
	{
		/// <summary>
		/// The declaration node of the item
		/// </summary>
		public N Node { get; }

		public NodeSymbolItem(N node, S symbol, int declarationOrder)
			: base(symbol, declarationOrder)
		{
			Node = node.CheckIfNull(nameof(node));
		}
	}
}
