#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Acuminator.Vsix.ToolWindows.Converters;

/// <summary>
/// Converter which converts <see cref="object"/> value to its type.
/// </summary>
[ValueConversion(typeof(object), typeof(Type))]
public class ObjectToItsTypeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
		value?.GetType() ?? Binding.DoNothing;

	public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => 
		throw new NotSupportedException();
}
