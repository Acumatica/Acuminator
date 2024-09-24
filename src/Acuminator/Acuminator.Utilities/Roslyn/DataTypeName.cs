using System;

using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Utilities.Roslyn;

public readonly record struct DataTypeName(string Value)
{
	public string Value { get; } = Value.RemoveEmptySpacesInArrayTypeNames();

	public override string ToString() => Value;
}
