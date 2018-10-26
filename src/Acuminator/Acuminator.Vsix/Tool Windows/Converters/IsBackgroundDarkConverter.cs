using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;


namespace Acuminator.Vsix.ToolWindows.Converters
{
	/// <summary>
	/// Converter which converts background <see cref="Brush"/> to <see cref="bool"/> if it is dark.
	/// </summary>
	[ValueConversion(typeof(Brush), typeof(bool))]
	public class IsBackgroundDarkConverter : IValueConverter
	{
		private const byte RedCriteria = 128;
		private const byte GreenCriteria = 128;
		private const byte BlueCriteria = 128;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is SolidColorBrush brush))
				return null;

			return brush.Color.R < RedCriteria ||  brush.Color.G < GreenCriteria || brush.Color.B < BlueCriteria;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
