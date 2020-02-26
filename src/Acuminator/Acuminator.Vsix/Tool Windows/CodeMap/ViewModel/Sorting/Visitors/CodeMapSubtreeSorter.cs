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
		protected NodesSorter NodesSorter { get; }

		protected CodeMapSortContext SortContext
		{
			get;
			set;
		}

		public CodeMapSubtreeSorter(NodesSorter nodesSorter = null)
		{
			NodesSorter = nodesSorter ?? new NodesSorter();
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
				subTreeRoot.Tree.RefreshFlattenedNodesList();
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
			else if (node.Children.Count > 1)  //Optimization for single element collections - do not sort and reset them
			{
				var sorted = NodesSorter.SortNodes(node.Children, SortContext.SortType, SortContext.SortDirection)
								?.ToList(capacity: node.Children.Count)
								?? Enumerable.Empty<TreeNodeViewModel>();

				node.Children.Reset(sorted);
			}

			if (SortContext.SortDescendants)
			{
				base.DefaultVisit(node);
			}
		}

		public override void VisitNode(AttributeNodeViewModel attributeNode)
		{
			//Optimization for attributes - don't put more on execution stack by visiting them
			attributeNode.ChildrenSortType = SortContext.SortType;
			attributeNode.ChildrenSortDirection = SortContext.SortDirection;
		}
	}
}