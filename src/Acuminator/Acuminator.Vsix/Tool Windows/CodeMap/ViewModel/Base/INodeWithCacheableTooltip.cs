#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for code map node with cacheable tooltip.
	/// </summary>
	public interface INodeWithCacheableTooltip
	{
		string? CalculateTooltip();
	}
}
