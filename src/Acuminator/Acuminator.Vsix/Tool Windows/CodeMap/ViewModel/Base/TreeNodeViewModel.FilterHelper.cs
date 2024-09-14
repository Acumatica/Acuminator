#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract partial class TreeNodeViewModel : ViewModelBase
	{
		protected static class FilterHelper
		{
			public static void RefreshSubTreeWithoutFilter(TreeNodeViewModel root, List<TreeNodeViewModel> nodesAndSelfFromRootToChildren)
			{
				var nodesAndSelfFromChildrenToRoot = (nodesAndSelfFromRootToChildren as IEnumerable<TreeNodeViewModel>).Reverse();

				foreach (var node in nodesAndSelfFromChildrenToRoot)
				{
					node.IsVisible = true;

					if (node.AllChildren.Count > 0)
						node._mutableDisplayedChildren.Reset(node.AllChildren);
				}

				var ancestorsToUpdateToRefreshFromDescendantToRoot = 
					root.Ancestors()
						.Where(ancestor => ancestor.FilterBehavior == TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter)
						.ToList(capacity: 8);

				foreach (var ancestor in ancestorsToUpdateToRefreshFromDescendantToRoot)
				{
					ancestor.IsVisible = true;

					if (ancestor.AllChildren.Count > 0)
						ancestor._mutableDisplayedChildren.Reset(ancestor.AllChildren);
				}

				RefreshDetailsVisibility(ancestorsToUpdateToRefreshFromDescendantToRoot, nodesAndSelfFromRootToChildren);
			}

			public static void RefreshSubTreeWithFilter(TreeNodeViewModel root, List<TreeNodeViewModel> nodesAndSelfFromRootToChildren,
														FilterOptions filterOptions)
			{
				var nodesAndSelfFromChildrenToRoot = (nodesAndSelfFromRootToChildren as IEnumerable<TreeNodeViewModel>).Reverse();

				foreach (var node in nodesAndSelfFromChildrenToRoot)
				{
					node.IsVisible = IsNodeMatchedWithFilter(node, filterOptions);

					if (node.AllChildren.Count > 0)
						node.RefreshDisplayedChildren();
				}

				var ancestorsToUpdateToRefreshFromDescendantToRoot =
					root.Ancestors()
						.Where(ancestor => ancestor.FilterBehavior == TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter)
						.ToList(capacity: 8);

				foreach (var ancestor in ancestorsToUpdateToRefreshFromDescendantToRoot)
				{
					ancestor.IsVisible = IsNodeMatchedWithFilter(ancestor, filterOptions);

					if (ancestor.AllChildren.Count > 0)
						ancestor.RefreshDisplayedChildren();
				}

				RefreshDetailsVisibility(ancestorsToUpdateToRefreshFromDescendantToRoot, nodesAndSelfFromRootToChildren);
			}

			private static bool IsNodeMatchedWithFilter(TreeNodeViewModel node, FilterOptions filterOptions)
			{
				switch (node.FilterBehavior)
				{
					case TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter:
						return node.NameMatchesPattern(filterOptions.FilterPattern) ||
							   (node.AllChildren.Count > 0 && node.AllChildren.Any(child => child.IsVisible));
					case TreeNodeFilterBehavior.AlwaysHidden:
						return false;
					case TreeNodeFilterBehavior.AlwaysDisplayed:
					default:
						return true;
				}
			}

			private static void RefreshDetailsVisibility(List<TreeNodeViewModel> ancestorsToUpdateToRefreshFromDescendantToRoot,
														 List<TreeNodeViewModel> nodesAndSelfFromRootToChildren)
			{
				if (ancestorsToUpdateToRefreshFromDescendantToRoot.Count > 0)
				{
					var ancestorsToUpdateToRefreshFromRootToDescendant =
						(ancestorsToUpdateToRefreshFromDescendantToRoot as IEnumerable<TreeNodeViewModel>).Reverse();

					foreach (var ancestor in ancestorsToUpdateToRefreshFromRootToDescendant)
						ancestor.NotifyPropertyChanged(nameof(AreDetailsVisible));
				}

				if (nodesAndSelfFromRootToChildren.Count > 0)
				{
					foreach (var node in nodesAndSelfFromRootToChildren)
						node.NotifyPropertyChanged(nameof(AreDetailsVisible));
				}
			}
		}
	}
}
