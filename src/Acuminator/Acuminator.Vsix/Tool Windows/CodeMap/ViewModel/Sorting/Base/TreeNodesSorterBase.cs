using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes sorter base class.
	/// </summary>
	public abstract class TreeNodesSorterBase
	{
		public List<TreeNodeViewModel> SortNodes(IEnumerable<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty())
				return new List<TreeNodeViewModel>();

			var sortedNodes = new List<TreeNodeViewModel>(capacity: 8);
			var sortableNodes = new List<TreeNodeViewModel>(capacity: 8);

			foreach (TreeNodeViewModel node in nodes)
			{
				if (!node.IsSortTypeSupported(sortType))
					sortedNodes.Add(node);
				else
					sortableNodes.Add(node);
			}

			var sortResult = SortNodesBySortTypeAndDirection(sortableNodes, sortType, sortDirection);
			sortedNodes.AddRange(sortResult);
			return sortedNodes;
		}

		public IEnumerable<TreeNodeViewModel> SortNodes(IReadOnlyCollection<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty())
				yield break;

			int sortableCount = nodes.Count;

			foreach (TreeNodeViewModel node in nodes)
			{
				if (!node.IsSortTypeSupported(sortType))
				{
					sortableCount--;
					yield return node;
				}
			}

			if (sortableCount == 0)
				yield break;

			var sortableNodes = nodes.Where(child => child.IsSortTypeSupported(sortType));
			var sortedNodes = SortNodesBySortTypeAndDirection(sortableNodes, sortType, sortDirection);

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