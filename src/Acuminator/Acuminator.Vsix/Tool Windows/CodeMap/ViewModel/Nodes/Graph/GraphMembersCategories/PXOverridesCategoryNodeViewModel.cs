#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PXOverridesCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public PXOverridesCategoryNodeViewModel(GraphNodeViewModel graphViewModel, TreeNodeViewModel parent,
												ExpandCreatedNodesFunction isExpandedCalculator) : 
										   base(graphViewModel, parent, GraphMemberCategory.PXOverride, isExpandedCalculator)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => CodeMapGraphModel.GraphModel.DeclaredPXOverrides;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
