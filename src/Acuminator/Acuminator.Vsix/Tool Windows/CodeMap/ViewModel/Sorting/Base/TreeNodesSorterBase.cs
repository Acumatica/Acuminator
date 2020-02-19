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
	public abstract class TreeNodesSorterBase : CodeMapTreeVisitor<IEnumerable<TreeNodeViewModel>>
	{
		protected SortType SortType
		{
			get;
			set;
		}

		protected SortDirection SortDirection
		{
			get;
			set;
		}

		protected TreeNodesSorterBase() : base(Enumerable.Empty<TreeNodeViewModel>())
		{
		}

		/// <summary>
		/// This method checks if the node can be sorted with the specified <paramref name="sortType"/> and reordered by sorting of code map nodes.
		/// </summary>
		/// <param name="nodeViewModel">The node view model.</param>
		/// <param name="sortType">Type of the sort.</param>
		/// <returns/>
		protected abstract bool IsSortTypeSupported(TreeNodeViewModel nodeViewModel, SortType sortType);

		public override IEnumerable<TreeNodeViewModel> DefaultVisit(TreeNodeViewModel nodeViewModel, CancellationToken cancellation)
		{
			return base.DefaultVisit(nodeViewModel, cancellation);
		}

		public virtual void SortSubtree(TreeNodeViewModel subTreeRoot, SortType sortType, SortDirection sortDirection, bool sortDescendants)
		{
			sorter.ThrowOnNull(nameof(sorter));

			ChildrenSortType = sortType;
			ChildrenSortDirection = sortDirection;
			var sorted = sorter.SortNodes(Children, sortType, sortDirection).ToList(capacity: Children.Count) ?? Enumerable.Empty<TreeNodeViewModel>();

			Children.Reset(sorted);

			if (sortDescendants && Children.Count > 0)
			{
				foreach (var childNode in Children)
				{
					childNode.SortSubtree(sorter, sortType, sortDirection, sortDescendants);
				}
			}
		}



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

		protected virtual IEnumerable<TreeNodeViewModel> SortNodesBySortTypeAndDirection(IEnumerable<TreeNodeViewModel> nodesToSort)
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