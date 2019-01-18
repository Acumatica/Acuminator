using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace Acuminator.Vsix.ToolWindows.Converters
{
	/// <summary>
	/// Converter, which inverts bool values.
	/// </summary>
	[ValueConversion(sourceType: typeof(bool), targetType: typeof(bool))]
	public class BoolInversionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Invert(value);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Invert(value);

		private static object Invert(object value) =>
			value is bool boolean
				? !boolean
				: Binding.DoNothing;
	}
}
