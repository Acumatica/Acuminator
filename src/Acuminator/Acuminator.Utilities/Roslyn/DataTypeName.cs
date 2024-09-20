using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn;

public readonly record struct DataTypeName(string Value)
{
	public string Value { get; } = RemoveEmptySpacesInArrayTypeNames(Value);

	public override string ToString() => Value;

	private static string RemoveEmptySpacesInArrayTypeNames(string dataTypeName)
	{
		dataTypeName.ThrowOnNullOrWhiteSpace();

		bool isArrayTypeName = dataTypeName[^1] == ']';

		if (!isArrayTypeName)
			return dataTypeName;
		else if (dataTypeName.Length <= 2)
			throw new ArgumentException($"Invalid data type name \"{dataTypeName}\"", nameof(dataTypeName));

		// if there is no empty spaces in array, return the original string
		if (dataTypeName[^2] == '[' && !char.IsWhiteSpace(dataTypeName[^3]))
			return dataTypeName;

		int indexOfOpeningSquareBracket = dataTypeName.LastIndexOf('[');

		if (indexOfOpeningSquareBracket < 0)
			throw new ArgumentException($"Invalid data type name \"{dataTypeName}\"", nameof(dataTypeName));

		var elementTypeName = dataTypeName[..indexOfOpeningSquareBracket].Trim();
		return $"{elementTypeName}[]";
	}
}
