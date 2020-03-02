using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;


namespace Acuminator.Vsix.ToolWindows.Converters
{
	/// <summary>
	/// Converter, which converts <see cref="Color?" to <see cref="SolidColorBrush"/>/>.
	/// </summary>
	[ValueConversion(sourceType: typeof(Color?), targetType: typeof(SolidColorBrush))]
	public class ColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = value as Color?;
			return color.HasValue
				? new SolidColorBrush(color.Value)
				: null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is SolidColorBrush brush
				? brush.Color
				: (Color?)null;
	}
}
