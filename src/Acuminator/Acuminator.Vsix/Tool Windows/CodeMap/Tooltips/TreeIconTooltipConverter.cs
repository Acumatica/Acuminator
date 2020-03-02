using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the icon tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeIconTooltipConverter : IValueConverter
	{
		private const string IsKeyTooltip = "Is part of a primary key";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case PropertyNodeViewModel propertyNode when propertyNode.IsKey:
					return IsKeyTooltip;
				default:
					return Binding.DoNothing;
			}
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
