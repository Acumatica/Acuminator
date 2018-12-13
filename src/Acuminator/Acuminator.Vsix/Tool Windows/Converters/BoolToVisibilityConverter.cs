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
	/// Converter which converts <see cref="bool"/> bool values to the <see cref="Visibility"/> values.
	/// If converter parameter's value is "!" then the value is inverted.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(string))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		private const string InverseVisibilityValue = "!";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool visible))
				return Visibility.Collapsed;

			if (InverseVisibilityValue.Equals(parameter as string, StringComparison.Ordinal))
			{
				visible = !visible;
			}

			return visible ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
