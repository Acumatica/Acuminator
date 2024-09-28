using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for a Code Map placeholder node that represents symbol which data is not collected yet.
	/// </summary>
	public interface IPlaceholderNode
	{
		/// <summary>
		/// The placeholder symbol represented by the node.
		/// </summary>
		INamedTypeSymbol PlaceholderSymbol { get; }

		/// <summary>
		/// The declaration order of the placeholder symbol.
		/// </summary>
		int PlaceholderSymbolDeclarationOrder { get; }
	}
}
