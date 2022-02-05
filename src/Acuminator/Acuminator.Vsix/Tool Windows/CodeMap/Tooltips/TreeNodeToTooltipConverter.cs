using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeNodeToTooltipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TreeNodeViewModel treeNode))
				return Binding.DoNothing;

			string tooltip = treeNode.Tooltip;

			if (tooltip.IsNullOrWhiteSpace())
				return Binding.DoNothing;

			return tooltip.TrimExcess();
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
