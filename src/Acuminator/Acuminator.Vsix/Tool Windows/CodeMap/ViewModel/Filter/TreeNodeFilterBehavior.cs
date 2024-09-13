#nullable enable

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

/// <summary>
/// Values that represent different tree node behavior on filtering.
/// </summary>
public enum TreeNodeFilterBehavior
{
	DisplayedIfFilterMet,
	DisplayedIfFilterNotMet,
	AlwaysDisplayed,
	AlwaysHidden
}
