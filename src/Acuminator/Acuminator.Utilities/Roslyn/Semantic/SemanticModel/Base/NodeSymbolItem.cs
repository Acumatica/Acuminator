#nullable enable
using System.Diagnostics.CodeAnalysis;

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
		/// True if this symbol is declared in external assembly metadata, false if not.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => Node is null;

		/// <summary>
		/// True if this symbol is declared in the source code, false if not.
		/// </summary>
		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => Node is not null;

		/// <summary>
		/// The declaration node of the item
		/// </summary>
		public N? Node { get; }

		public NodeSymbolItem(N? node, S symbol, int declarationOrder)
			: base(symbol, declarationOrder)
		{
			Node = node;
		}
	}
}
