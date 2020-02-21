using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes special sorter implementation used to sort tree during tree construction.
	/// </summary>
	public class CodeMapTreeInitialSorter : CodeMapTreeVisitor<IEnumerable<TreeNodeViewModel>>
	{
		protected NodesSorter NodesSorter { get; }

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

		public CodeMapTreeInitialSorter(SortType defaultSortType, SortDirection defaultSortDirection, NodesSorter nodesSorter = null) :
								   base(defaultValue: Enumerable.Empty<TreeNodeViewModel>())
		{
			NodesSorter = nodesSorter ?? new NodesSorter();

			SortType = defaultSortType;
			SortDirection = defaultSortDirection;
		}

		public override IEnumerable<TreeNodeViewModel> DefaultVisit(TreeNodeViewModel node)
		{
			if (node == null)
				return base.DefaultVisit(node);

			node.ChildrenSortType = SortType;
			node.ChildrenSortDirection = SortDirection;

			return node.Children.Count > 0
				? NodesSorter.SortNodes(node.Children, SortType, SortDirection)
				: base.DefaultVisit(node);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode) =>
			VisitNodeWithCustomSortType(dacGroupingNode, SortType.Alphabet);

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode) =>
			VisitNodeWithCustomSortType(dacGroupingNode, SortType.Alphabet);

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode) =>
			VisitNodeWithCustomSortType(dacGroupingNode, SortType.Alphabet);

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode) =>
			VisitNodeWithCustomSortType(dacFieldGroupingNode, SortType.Alphabet);


		protected IEnumerable<TreeNodeViewModel> VisitNodeWithCustomSortType(TreeNodeViewModel node, SortType customSortType) =>
			VisitNodeWithCustomSortTypeAndDirection(node, customSortType, SortDirection);

		protected IEnumerable<TreeNodeViewModel> VisitNodeWithCustomSortDirection(TreeNodeViewModel node, SortDirection customSortDirection) =>
			VisitNodeWithCustomSortTypeAndDirection(node, SortType, customSortDirection);

		protected IEnumerable<TreeNodeViewModel> VisitNodeWithCustomSortTypeAndDirection(TreeNodeViewModel node, SortType customSortType,
																						 SortDirection customSortDirection)
		{
			SortDirection oldSortDirection = SortDirection;
			SortType oldSortType = SortType;

			try
			{
				SortDirection = customSortDirection;
				SortType = customSortType;
				return node.AcceptVisitor(this);
			}
			finally
			{
				SortDirection = oldSortDirection;
				SortType = oldSortType;
			}
		}
	}
}