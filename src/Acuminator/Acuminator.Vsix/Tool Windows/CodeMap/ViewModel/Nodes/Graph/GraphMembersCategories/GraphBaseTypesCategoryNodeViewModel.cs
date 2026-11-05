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

		public IReadOnlyList<GraphExtensionInfo> BaseGraphExtensionInfos { get; }

		public GraphBaseTypesCategoryNodeViewModel(GraphNodeViewModel graphNodeViewModel, TreeNodeViewModel parent,
												   ExpandCreatedNodesFunction isExpandedCalculator) : 
											  base(graphNodeViewModel, parent, GraphMemberCategory.BaseTypes, isExpandedCalculator)
		{
			if (GraphViewModel.CodeMapGraphModel.GraphType == GraphType.PXGraph)
			{
				BaseGraphInfo = GraphViewModel.CodeMapGraphModel.GraphInfo?.Base;
				BaseGraphExtensionInfos = Array.Empty<GraphExtensionInfo>();
			}
			else
			{
				BaseGraphInfo = GraphViewModel.CodeMapGraphModel.GraphInfo;
				var baseGraphExtensions = GraphViewModel.CodeMapGraphModel.GraphExtensionInfo?.BaseGraphExtensions;
				BaseGraphExtensionInfos = baseGraphExtensions != null && !baseGraphExtensions.Value.IsDefaultOrEmpty
					? baseGraphExtensions.Value
					: Array.Empty<GraphExtensionInfo>();
			}
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			(BaseGraphInfo != null, BaseGraphExtensionInfos.Count > 0) switch
			{
				(true, true)   => [BaseGraphInfo!, ..BaseGraphExtensionInfos],
				(true, false)  => [BaseGraphInfo!],
				(false, true)  => BaseGraphExtensionInfos,
				(false, false) => []
			};

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
