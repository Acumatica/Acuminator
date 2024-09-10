#nullable enable

using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

public class FilterEventArgs : EventArgs
{
	public string? NewFilterText { get; }

	public string? OldFilterText { get; }

	public FilterEventArgs(string? newFilterText, string? oldFilterText)
	{
		NewFilterText = newFilterText;
		OldFilterText = oldFilterText;
	}
}
