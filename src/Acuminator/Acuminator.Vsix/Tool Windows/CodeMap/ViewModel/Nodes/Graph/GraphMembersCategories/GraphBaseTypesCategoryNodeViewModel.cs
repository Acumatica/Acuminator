#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphBaseTypesCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public override Icon NodeIcon => Icon.BaseTypesCategory;

		protected override bool AllowNavigation => false;

		public override bool IconDependsOnCurrentTheme => true;

		public GraphInfo? BaseGraphInfo { get; }

		public GraphExtensionInfo? BaseGraphExtensionInfo { get; }

		public GraphBaseTypesCategoryNodeViewModel(GraphNodeViewModel graphNodeViewModel, TreeNodeViewModel parent, bool isExpanded) : 
											  base(graphNodeViewModel, parent, GraphMemberCategory.BaseTypes, isExpanded)
		{
			if (GraphViewModel.CodeMapGraphModel.GraphType == GraphType.PXGraph)
			{
				BaseGraphInfo = GraphViewModel.CodeMapGraphModel.GraphInfo?.Base;
				BaseGraphExtensionInfo = null;
			}
			else
			{
				BaseGraphInfo = GraphViewModel.CodeMapGraphModel.GraphInfo;
				BaseGraphExtensionInfo = GraphViewModel.CodeMapGraphModel.GraphExtensionInfo?.Base;
			}
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			(BaseGraphInfo != null, BaseGraphExtensionInfo != null) switch
			{
				(true, true)   => [BaseGraphInfo!, BaseGraphExtensionInfo!],
				(true, false)  => [BaseGraphInfo!],
				(false, true)  => [BaseGraphExtensionInfo!],
				(false, false) => []
			};

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
