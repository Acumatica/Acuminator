using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes initial sorter after the tree is built.
	/// </summary>
	public class CodeMapTreeInitialSorter : CodeMapTreeSorterBase
	{
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

		public override void VisitNode(AttributeNodeViewModel attributeNode)
		{
			//Stop visit for better performance
		}
	}
}