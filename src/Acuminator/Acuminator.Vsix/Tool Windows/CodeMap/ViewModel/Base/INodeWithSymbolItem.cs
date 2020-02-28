using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for code map node with symbol.
	/// </summary>
	public interface INodeWithSymbolItem
	{
		/// <summary>
		/// The symbol item reperesented by node.
		/// </summary>
		SymbolItem Symbol { get; }
	}
}
