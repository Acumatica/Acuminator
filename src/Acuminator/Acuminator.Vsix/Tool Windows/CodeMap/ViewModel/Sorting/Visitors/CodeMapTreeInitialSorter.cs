#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes special sorter implementation used to sort tree during tree construction to reduce a number of node.Children.Reset calls.
	/// </summary>
	public class CodeMapTreeInitialSorter : CodeMapTreeVisitor<IReadOnlyCollection<TreeNodeViewModel>, IReadOnlyCollection<TreeNodeViewModel>>
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

		public CodeMapTreeInitialSorter(SortType defaultSortType, SortDirection defaultSortDirection, NodesSorter? nodesSorter = null) :
								   base(defaultValue: [])
		{
			NodesSorter = nodesSorter ?? new NodesSorter();

			SortType = defaultSortType;
			SortDirection = defaultSortDirection;
		}

		public IReadOnlyCollection<TreeNodeViewModel> SortGeneratedChildren(TreeNodeViewModel parentNode, 
															 IReadOnlyCollection<TreeNodeViewModel> generatedChildren) =>
			VisitNode(parentNode, generatedChildren);

		public override IReadOnlyCollection<TreeNodeViewModel> DefaultVisit(TreeNodeViewModel node, 
																IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
		{
			if (node == null)
				return DefaultValue;

			SetTreeNodeSortProperties(node);
			
			return !generatedChildren.IsNullOrEmpty()
				? NodesSorter.SortNodes(generatedChildren, SortType, SortDirection)
							 .ToList(capacity: generatedChildren.Count)
				: base.DefaultVisit(node, generatedChildren);
		}

		//Optimization for attributes - don't put more on execution stack by visiting them
		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(CacheAttachedAttributeNodeViewModel attributeNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren) =>
			SetTreeNodeSortProperties(attributeNode);

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacAttributeNodeViewModel attributeNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren) =>
			SetTreeNodeSortProperties(attributeNode);

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacFieldAttributeNodeViewModel attributeNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren) =>
			SetTreeNodeSortProperties(attributeNode);

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(GraphAttributeNodeViewModel attributeNode,
															IReadOnlyCollection<TreeNodeViewModel> generatedChildren) =>
			SetTreeNodeSortProperties(attributeNode);

		private IReadOnlyCollection<TreeNodeViewModel> SetTreeNodeSortProperties(TreeNodeViewModel node)
		{
			node.ChildrenSortType	   = SortType;
			node.ChildrenSortDirection = SortDirection;
			return DefaultValue;
		}

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode,
																IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(attributeGroupNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(GraphAttributesGroupNodeViewModel attributeGroupNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
		{
			SortType oldSortType = SortType;

			try
			{
				SortType = SortType.Alphabet;
				return base.VisitNode(attributeGroupNode, generatedChildren);
			}
			finally
			{
				SortType = oldSortType;
			}
		}

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
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

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
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

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
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

		public override IReadOnlyCollection<TreeNodeViewModel> VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode,
														  IReadOnlyCollection<TreeNodeViewModel> generatedChildren)
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