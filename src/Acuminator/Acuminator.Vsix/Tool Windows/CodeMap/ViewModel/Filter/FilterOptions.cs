#nullable enable

using System.Diagnostics.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

public record FilterOptions(string? FilterPattern)
{
	public static readonly FilterOptions NoFilter = new FilterOptions((string?)null); 

	[MemberNotNullWhen(returnValue: true, nameof(FilterPattern))]
	public bool HasFilter => FilterPattern != null;

	public string? FilterPattern { get; } = FilterPattern.IsNullOrEmpty() ? null : FilterPattern;
}
