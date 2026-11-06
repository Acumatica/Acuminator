#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedCategoryNodeViewModel : GraphEventHandlerCategoryNodeViewModel
	{
		public CacheAttachedCategoryNodeViewModel(GraphNodeViewModel graphViewModel, TreeNodeViewModel parent, 
												  ExpandCreatedNodesFunction isExpandedCalculator) :
											 base(graphViewModel, parent, GraphMemberCategory.CacheAttached, isExpandedCalculator)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.DeclaredEventHandlers.CacheAttachedEventHandlers;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
