#nullable enable

using System;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

public class FilterEventArgs : EventArgs
{
	public FilterOptions FilterOptions { get; }

	public string? NewFilterText => FilterOptions.FilterPattern;

	public string? OldFilterText { get; }

	public FilterEventArgs(FilterOptions filterOptions, string? oldFilterText)
	{
		FilterOptions = filterOptions.CheckIfNull();
		OldFilterText = oldFilterText;
	}
}
