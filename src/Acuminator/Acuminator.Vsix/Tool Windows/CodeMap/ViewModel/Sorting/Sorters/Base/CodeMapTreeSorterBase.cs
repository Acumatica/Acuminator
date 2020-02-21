using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes sorter base class.
	/// </summary>
	public abstract class CodeMapTreeSorterBase : CodeMapTreeWalker
	{
		protected CodeMapSortContext SortContext
		{
			get;
			set;
		}

		public void SortChildren(TreeNodeViewModel node, SortType sortType, SortDirection sortDirection) =>
			SortSubtree(node, sortType, sortDirection, sortDescendants: false);

		public void SortSubtree(TreeNodeViewModel subTreeRoot, SortType sortType, SortDirection sortDirection) =>
			SortSubtree(subTreeRoot, sortType, sortDirection, sortDescendants: true);

		protected void SortSubtree(TreeNodeViewModel subTreeRoot, SortType sortType, SortDirection sortDirection, bool sortDescendants)
		{
			if (subTreeRoot == null)
				return;

			try
			{
				SortContext = new CodeMapSortContext(sortType, sortDirection, sortDescendants);
				VisitNode(subTreeRoot);
			}
			finally
			{
				SortContext = null;
			}		
		}

		public override void DefaultVisit(TreeNodeViewModel node)
		{
			if (node == null || SortContext == null)
				return;

			node.ChildrenSortType = SortContext.SortType;
			node.ChildrenSortDirection = SortContext.SortDirection;

			if (node.Children.Count == 0)
				return;

			var sorted = SortNodes(node.Children, SortContext.SortType, SortContext.SortDirection)
							?.ToList(capacity: node.Children.Count) 
							?? Enumerable.Empty<TreeNodeViewModel>();

			node.Children.Reset(sorted);
			base.DefaultVisit(node);
		}

		/// <summary>
		/// This method checks if the node can be sorted with the specified <paramref name="sortType"/> and reordered by sorting of code map nodes.
		/// </summary>
		/// <param name="node">The node view model.</param>
		/// <param name="sortType">Type of the sort.</param>
		/// <returns/>
		protected virtual bool IsSortTypeSupported(TreeNodeViewModel node, SortType sortType) => 
			sortType == SortType.Alphabet || sortType == SortType.Declaration;

		protected List<TreeNodeViewModel> SortNodes(IEnumerable<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
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

			var sortResult = SortNodesBySortTypeAndDirection(sortableNodes, sortType, sortDirection);
			sortedNodes.AddRange(sortResult);
			return sortedNodes;
		}

		protected IEnumerable<TreeNodeViewModel> SortNodes(IReadOnlyCollection<TreeNodeViewModel> nodes, SortType sortType, SortDirection sortDirection)
		{
			if (nodes.IsNullOrEmpty())
				yield break;

			int sortableCount = nodes.Count;

			foreach (TreeNodeViewModel node in nodes)
			{
				if (IsSortTypeSupported(node, sortType))
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