#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.Common
{
	/// <summary>
	/// Information about the tooltip and how to display it.
	/// </summary>
	public class TooltipInfo
	{
		/// <summary>
		/// Gets the tooltip text.
		/// </summary>
		/// <value>
		/// The tooltip text.
		/// </value>
		public string Tooltip { get; }

		/// <summary>
		/// Gets or sets a value indicating whether to trim excess <see cref="Tooltip"/>.
		/// </summary>
		/// <value>
		/// True to trim excess tooltip text, false to not trim.
		/// </value>
		public bool TrimExcess { get; set; }

		/// <summary>
		/// Gets or sets the maximum length of tooltup text for trimming.
		/// </summary>
		/// <value>
		/// The maximum length of tooltup text for trimming.
		/// </value>
		public int? MaxLength { get; set; }

		/// <summary>
		/// Gets or sets the overflow suffix added to the end of trimmed tooltip.
		/// </summary>
		/// <value>
		/// The overflow suffix added to the end of trimmed tooltip.
		/// </value>
		public string? OverflowSuffix { get; set; }

		public TooltipInfo(string tooltip)
		{
			Tooltip = tooltip.NullIfWhiteSpace().CheckIfNull(nameof(tooltip));
		}
	}
}
