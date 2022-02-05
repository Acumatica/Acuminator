using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ToolWindows.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="IElementWithTooltip"/> UI element to tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(IElementWithTooltip), targetType: typeof(string))]
	public class ElementWithTooltipToTooltipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is IElementWithTooltip elementWithTooltip))
				return Binding.DoNothing;

			TooltipInfo tooltipInfo = elementWithTooltip.CalculateTooltip();

			if (tooltipInfo == null)
				return Binding.DoNothing;

			if (tooltipInfo.TrimExcess)
				return tooltipInfo.Tooltip.TrimExcess(tooltipInfo.MaxLength, tooltipInfo.OverflowSuffix);		
			else
				return tooltipInfo.Tooltip;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
