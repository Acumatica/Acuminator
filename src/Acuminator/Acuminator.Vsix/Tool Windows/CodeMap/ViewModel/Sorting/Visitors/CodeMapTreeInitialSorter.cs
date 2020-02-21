using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes special sorter implementation used to sort tree during tree construction to reduce a number of node.Children.Reset calls.
	/// </summary>
	public class CodeMapTreeInitialSorter : CodeMapTreeVisitor<IEnumerable<TreeNodeViewModel>, List<TreeNodeViewModel>>
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
								   base(defaultValue: new List<TreeNodeViewModel>())
		{
			NodesSorter = nodesSorter ?? new NodesSorter();

			SortType = defaultSortType;
			SortDirection = defaultSortDirection;
		}

		public List<TreeNodeViewModel> SortGeneratedChildren(TreeNodeViewModel parentNode, IEnumerable<TreeNodeViewModel> generatedChildren) =>
			VisitNode(parentNode, generatedChildren);

		public override List<TreeNodeViewModel> DefaultVisit(TreeNodeViewModel node, IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			if (node == null)
				return base.DefaultVisit(node, generatedChildren);

			node.ChildrenSortType = SortType;
			node.ChildrenSortDirection = SortDirection;

			return !generatedChildren.IsNullOrEmpty()
				? NodesSorter.SortNodes(generatedChildren, SortType, SortDirection)
				: base.DefaultVisit(node, generatedChildren);
		}

		public override List<TreeNodeViewModel> VisitNode(AttributeNodeViewModel attributeNode, IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			return DefaultValue;  //Optimization for attributes
		}

		public override List<TreeNodeViewModel> VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode,
														  IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(dacGroupingNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}

		public override List<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode,
														  IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(dacGroupingNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}

		public override List<TreeNodeViewModel> VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode,
														  IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(dacGroupingNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}

		public override List<TreeNodeViewModel> VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode,
														  IEnumerable<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(dacFieldGroupingNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}
	}
}