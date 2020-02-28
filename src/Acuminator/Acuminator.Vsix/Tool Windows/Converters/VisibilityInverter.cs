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
	/// Converter which inverts <see cref="Visibility"/> values from <see cref="Visibility.Visible"/> to <see cref="Visibility.Collapsed"/>.
	/// </summary>
	[ValueConversion(sourceType: typeof(Visibility), targetType: typeof(Visibility))]
	public class VisibilityInverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Invert(value);

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => Invert(value);

		private static object Invert(object value)
		{
			switch (value)
			{
				case Visibility visibility when visibility == Visibility.Visible:
					return Visibility.Collapsed;
				case Visibility visibility when visibility == Visibility.Collapsed:
					return Visibility.Visible;
				default:
					return Binding.DoNothing;
			}
		}
	}
}
