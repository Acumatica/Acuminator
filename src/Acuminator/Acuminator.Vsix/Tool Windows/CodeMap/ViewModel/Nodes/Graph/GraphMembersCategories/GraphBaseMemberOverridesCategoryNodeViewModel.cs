#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphBaseMemberOverridesCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public override Icon NodeIcon => Icon.BaseMemberOverrideGraphCategory;

		public GraphBaseMemberOverridesCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
														base(graphViewModel, GraphMemberType.BaseMemberOverride, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => CodeMapGraphModel.BaseMemberOverrides;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
