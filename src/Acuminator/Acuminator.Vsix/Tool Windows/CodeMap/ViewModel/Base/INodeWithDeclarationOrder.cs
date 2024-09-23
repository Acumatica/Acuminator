using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for Code Map node with a declaration order.
	/// </summary>
	public interface INodeWithDeclarationOrder
	{
		/// <summary>
		/// The declaration order.
		/// </summary>
		int DeclarationOrder { get; }
	}
}
