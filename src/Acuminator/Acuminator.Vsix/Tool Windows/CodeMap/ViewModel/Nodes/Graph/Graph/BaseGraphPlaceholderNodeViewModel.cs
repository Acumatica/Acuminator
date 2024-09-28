#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class BaseGraphPlaceholderNodeViewModel : GraphNodeViewModelBase, IPlaceholderNode
	{
		public GraphNodeViewModel ContainingGraphNode { get; }

		public GraphSemanticModelForCodeMap ParentGraphModel => ContainingGraphNode.CodeMapGraphModel;

		public override GraphOrGraphExtInfoBase GraphOrGraphExtInfo { get; }

		public override bool IsExpanderAlwaysVisible => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		#region IPlaceholderNode implementation
		INamedTypeSymbol IPlaceholderNode.PlaceholderSymbol => GraphOrGraphExtInfo.Symbol;

		int IPlaceholderNode.PlaceholderSymbolDeclarationOrder => GraphOrGraphExtInfo.DeclarationOrder;
		#endregion

		public BaseGraphPlaceholderNodeViewModel(GraphOrGraphExtInfoBase graphOrGraphExtInfo, GraphNodeViewModel containingGraphNode,
												 TreeNodeViewModel parent, bool isExpanded) :
											base(containingGraphNode.CheckIfNull().Tree, parent, isExpanded)
		{
			GraphOrGraphExtInfo = graphOrGraphExtInfo.CheckIfNull();
			ContainingGraphNode = containingGraphNode;

			var graphTypeInfo = CreateGraphTypeInfo();
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(graphTypeInfo);
		}

		protected override bool BeforeNodeExpansionChanged(bool oldValue, bool newValue)
		{
			base.BeforeNodeExpansionChanged(oldValue, newValue);

			return this.ReplacePlaceholderWithSubTreeOnExpansion(ContainingGraphNode.GraphSemanticModel.PXContext, isExpanding: newValue);
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) =>
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}