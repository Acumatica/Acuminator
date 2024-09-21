using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;

internal readonly record struct NotRedeclaredBqlFieldInfo(string DacFieldName, string NameOfBaseDacDeclaringBqlField, string BqlFieldName,
														  string? BqlFieldType, Location Location, bool IsReportedOnProperty)
{
	public string GetBqlFieldWithTypeDataString() => 
		$"{BqlFieldName}{Separators.TypePartSeparator}{BqlFieldType.NullIfWhiteSpace() ?? string.Empty}";
}