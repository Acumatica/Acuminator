using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeForCacheAttachedEventViewModel : DacGroupingNodeForFieldEventViewModel
	{
		public DacGroupingNodeForCacheAttachedEventViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName,
																IEnumerable<GraphFieldEventInfo> cacheAttachedEvents, bool isExpanded) :
														   base(graphEventsCategoryVM, dacName, cacheAttachedEvents, isExpanded)
		{
			ChildrenSortType = SortType.Declaration;
		}

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}