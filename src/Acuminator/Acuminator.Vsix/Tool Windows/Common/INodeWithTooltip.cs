#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.Common
{
	/// <summary>
	/// Interface for UI element with a tooltip.
	/// </summary>
	public interface IElementWithTooltip
	{
		TooltipInfo? CalculateTooltip();
	}
}
