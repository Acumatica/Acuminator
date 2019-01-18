using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;


namespace Acuminator.Vsix.ToolWindows.Converters
{
	/// <summary>
	/// Converter which converts <see cref="object"/> value to its type.
	/// </summary>
	[ValueConversion(typeof(object), typeof(Type))]
	public class ObjectToItsTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Binding.DoNothing;

			return value.GetType();
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
