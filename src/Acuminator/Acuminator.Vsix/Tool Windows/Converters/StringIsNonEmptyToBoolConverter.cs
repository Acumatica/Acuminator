#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.Converters;

/// <summary>
/// A one way converter which converts <see cref="string"/> values to the <see cref="bool"/> values.
/// Returns <see langword="true"/> if the string is not null or empty. Otherwise, returns <see langword="false"/>.
/// </summary>
[ValueConversion(typeof(string), typeof(bool))]
public class StringIsNonEmptyToBoolConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return false;

		string valueStr = (value as string) ?? value.ToString();
		return !valueStr.IsNullOrEmpty();
	}

	public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
