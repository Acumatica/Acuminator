using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes sorter.
	/// </summary>
	public class TreeNodesSorter
	{
		public virtual IEnumerable<TreeNodeViewModel> SortNodes(IReadOnlyCollection<TreeNodeViewModel> nodes, SortType sortType,
																SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty())
				yield break;

			int sortableCount = nodes.Count;

			foreach (TreeNodeViewModel node in nodes)
			{
				if (!node.IsSortable)
				{
					sortableCount--;
					yield return node;
				}
			}

			if (sortableCount == 0)
				yield break;

			var sortedNodes = nodes.Where(child => !child.IsSortable);
			sortedNodes = SortNodesBySortTypeAndDirection(sortedNodes, sortType, sortDirection);

			foreach (TreeNodeViewModel node in sortedNodes)
			{
				yield return node;
			}		
		}

		protected virtual IEnumerable<TreeNodeViewModel> SortNodesBySortTypeAndDirection(IEnumerable<TreeNodeViewModel> nodesToSort, SortType sortType,
																						 SortDirection sortDirection)
		{
			switch (sortType)
			{
				case SortType.Declaration:
					return sortDirection == SortDirection.Ascending
						? nodesToSort.OrderBy(NodeDeclarationOrderComparer.Instance)
						: nodesToSort.OrderByDescending(NodeDeclarationOrderComparer.Instance);

				case SortType.Alphabet:
					return sortDirection == SortDirection.Ascending
						? nodesToSort.OrderBy(node => node.Name)
						: nodesToSort.OrderByDescending(node => node.Name);

				default:
					return nodesToSort;
			}
		}	
	}
}