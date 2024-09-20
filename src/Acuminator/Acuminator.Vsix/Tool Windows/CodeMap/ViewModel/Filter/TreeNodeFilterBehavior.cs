#nullable enable

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

/// <summary>
/// Values that represent different tree node behavior on filtering.
/// </summary>
public enum TreeNodeFilterBehavior
{
	/// <summary>
	/// Tree node is displayed in Code Map if it or its children meet the filter criteria.
	/// </summary>
	DisplayedIfNodeOrChildrenMeetFilter,

	/// <summary>
	/// Tree node is displayed in Code Map if its children meet the filter criteria.
	/// </summary>
	DisplayedIfChildrenMeetFilter,

	/// <summary>
	/// Tree node is always displayed in Code Map no matter what the filter.
	/// </summary>
	AlwaysDisplayed,

	/// <summary>
	/// Tree node is always hidden in Code Map no matter what the filter.
	/// </summary>
	AlwaysHidden
}

public static class TreeNodeFilterBehaviorUtils
{
	public static bool DependsOnChildren(this TreeNodeFilterBehavior nodeFilterBehavior) =>
		nodeFilterBehavior == TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter ||
		nodeFilterBehavior == TreeNodeFilterBehavior.DisplayedIfChildrenMeetFilter;
}
