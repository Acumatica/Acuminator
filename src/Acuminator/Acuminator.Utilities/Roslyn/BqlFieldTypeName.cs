using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn;

public readonly record struct BqlFieldTypeName(string Value)
{
	public string Value { get; } = Value.CheckIfNullOrWhiteSpace();

	public override string ToString() => Value;

	public static implicit operator string(BqlFieldTypeName value) => value;

	public static explicit operator BqlFieldTypeName(string value) => new(value);
}
