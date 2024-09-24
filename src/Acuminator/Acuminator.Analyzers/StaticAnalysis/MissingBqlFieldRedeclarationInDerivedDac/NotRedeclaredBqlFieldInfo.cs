using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;

internal readonly record struct NotRedeclaredBqlFieldInfo(string DacFieldName, string NameOfBaseDacDeclaringBqlField, string BqlFieldName,
														  string? BqlFieldType, Location Location)
{
	public string GetBqlFieldWithTypeDataString() => 
		$"{BqlFieldName}{Constants.TypePartSeparator}{BqlFieldType.NullIfWhiteSpace() ?? string.Empty}";
}