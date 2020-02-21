using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A class responsible for nodes sorting. Contains resusable logic doing actual sorting
	/// </summary>
	public class NodesSorter
	{
		public List<TreeNodeViewModel> SortNodes(IEnumerable<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty())
				return new List<TreeNodeViewModel>();

			var sortedNodes = new List<TreeNodeViewModel>(capacity: 8);
			var sortableNodes = new List<TreeNodeViewModel>(capacity: 8);

			foreach (TreeNodeViewModel node in nodes)
			{
				if (!IsSortTypeSupported(node, sortType))
					sortedNodes.Add(node);
				else
					sortableNodes.Add(node);
			}

			//Optimization - do not sort for empty or single element collections
			switch (sortableNodes.Count)
			{
				case 0:
					return sortedNodes;

				case 1:
					sortedNodes.Add(sortableNodes[0]);
					return sortedNodes;

				default:
					var sortResult = SortNodesBySortTypeAndDirection(sortableNodes, sortType, sortDirection);
					sortedNodes.AddRange(sortResult);
					return sortedNodes;
			}
		}

		public IEnumerable<TreeNodeViewModel> SortNodes(IReadOnlyCollection<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty() || nodes.Count == 1)  //optimization - do not sort empty and single element collections and do not create iterator for them
				return nodes;

			return SortNodesImpl();

			//------------------------------------------Local function-------------------------------------------------------------
			IEnumerable<TreeNodeViewModel> SortNodesImpl()
			{
				int sortableCount = nodes.Count;

				foreach (TreeNodeViewModel node in nodes)
				{
					if (!IsSortTypeSupported(node, sortType))
					{
						sortableCount--;
						yield return node;
					}
				}

				if (sortableCount == 0)
					yield break;

				var sortableNodes = nodes.Where(child => IsSortTypeSupported(child, sortType));
				var sortedNodes = SortNodesBySortTypeAndDirection(sortableNodes, sortType, sortDirection);

				foreach (TreeNodeViewModel node in sortedNodes)
				{
					yield return node;
				}
			}
		}

		/// <summary>
		/// This method checks if the node can be sorted with the specified <paramref name="sortType"/> and reordered by sorting of code map nodes.
		/// </summary>
		/// <param name="node">The node view model.</param>
		/// <param name="sortType">Type of the sort.</param>
		/// <returns/>
		protected virtual bool IsSortTypeSupported(TreeNodeViewModel node, SortType sortType)
		{
			switch (node)
			{
				case DacMemberCategoryNodeViewModel _:
				case GraphMemberCategoryNodeViewModel _:
					return false;
				case DacGroupingNodeBaseViewModel _:
				case DacFieldGroupingNodeBaseViewModel _:
					return sortType == SortType.Alphabet;
				default:
					return sortType == SortType.Alphabet || sortType == SortType.Declaration;
			}
		}

		protected virtual IEnumerable<TreeNodeViewModel> SortNodesBySortTypeAndDirection(IEnumerable<TreeNodeViewModel> nodesToSort,
																						 SortType sortType, SortDirection sortDirection)
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