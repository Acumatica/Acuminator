#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

using Acuminator.Utilities.Common;
using System.Linq;

namespace Acuminator.Vsix.ToolWindows.Converters;

/// <summary>
/// Aggregation modes for conversion from multiple bools to a single bool.
/// </summary>
public enum MultipleBoolsAggregationMode 
{
	AnyTrue,
	AnyFalse,
	AllTrue,
	AllFalse
}

/// <summary>
/// Converter which converts multiple <see cref="bool"/> values to a single bool.
/// Converter parameter that specifies the conversion type is mandatory.
/// </summary>
public class MultipleBoolsToSingleBoolConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		parameter.ThrowOnNull();

		var valuesCastedToBool = values.OfType<bool>();

		if (GetAggregationModeFromConverterParameter(parameter) is not MultipleBoolsAggregationMode aggregationMode)
		{
			throw new NotSupportedException($"Converter parameter should be a value from the {nameof(MultipleBoolsAggregationMode)} enum. " +
											$"Value \"{parameter}\" is not supported.");
		}

		return aggregationMode switch
		{
			MultipleBoolsAggregationMode.AllTrue  => valuesCastedToBool.AllTrue(),
			MultipleBoolsAggregationMode.AllFalse => valuesCastedToBool.AllFalse(),
			MultipleBoolsAggregationMode.AnyTrue  => valuesCastedToBool.AnyTrue(),
			MultipleBoolsAggregationMode.AnyFalse => valuesCastedToBool.AnyFalse(),
			_ 									  => new NotSupportedException(
															$"{nameof(MultipleBoolsAggregationMode)} value \"{aggregationMode}\" is not supported.")
		};
	}

	private MultipleBoolsAggregationMode? GetAggregationModeFromConverterParameter(object converterParameter) =>
		converterParameter switch
		{
			MultipleBoolsAggregationMode aggregationMode 						 => aggregationMode,
			string parameterStr
			when Enum.TryParse(parameterStr, ignoreCase: true,
							   out MultipleBoolsAggregationMode aggregationMode) => aggregationMode,
			_ 																	 => null,
		};

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
