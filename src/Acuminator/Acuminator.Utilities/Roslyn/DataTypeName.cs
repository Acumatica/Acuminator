using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn;

public readonly record struct DataTypeName(string Value)
{
	public string Value { get; } = Value.CheckIfNullOrWhiteSpace();

	public override string ToString() => Value;
}
