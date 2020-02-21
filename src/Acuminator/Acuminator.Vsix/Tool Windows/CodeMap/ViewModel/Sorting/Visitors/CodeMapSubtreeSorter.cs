using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes sorter default implementation for sorting whole subtree.
	/// </summary>
	public class CodeMapSubtreeSorter : CodeMapTreeWalker
	{
		private readonly NodesSorter _nodesSorter;

		protected CodeMapSortContext SortContext
		{
			get;
			set;
		}

		public CodeMapSubtreeSorter(NodesSorter nodesSorter = null)
		{
			_nodesSorter = nodesSorter ?? new NodesSorter();
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

			var sorted = _nodesSorter.SortNodes(node.Children, SortContext.SortType, SortContext.SortDirection)
							?.ToList(capacity: node.Children.Count) 
							?? Enumerable.Empty<TreeNodeViewModel>();

			node.Children.Reset(sorted);

			if (SortContext.SortDescendants)
			{
				base.DefaultVisit(node);
			}
		}

		public override void VisitNode(AttributeNodeViewModel attributeNode)
		{
			//Stop visit for better performance
		}
	}
}