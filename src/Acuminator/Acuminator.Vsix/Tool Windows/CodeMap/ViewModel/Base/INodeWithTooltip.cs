#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for code map tree node with a tooltip.
	/// </summary>
	public interface INodeWithTooltip
	{
		TooltipInfo? CalculateTooltip();
	}
}
