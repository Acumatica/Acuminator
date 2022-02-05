#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for UI elements with a tooltip.
	/// </summary>
	public interface IElementWithTooltip
	{
		TooltipInfo? CalculateTooltip();
	}
}
