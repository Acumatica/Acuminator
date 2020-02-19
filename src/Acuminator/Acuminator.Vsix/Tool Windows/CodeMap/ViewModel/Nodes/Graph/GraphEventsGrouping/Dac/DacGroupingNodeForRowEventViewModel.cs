using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeForRowEventViewModel : DacGroupingNodeBaseViewModel
	{
		public ImmutableArray<GraphRowEventInfo> RowEvents { get; }

		public DacGroupingNodeForRowEventViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName,
												   IEnumerable<GraphRowEventInfo> rowEvents, bool isExpanded) :
											  base(graphEventsCategoryVM, dacName, isExpanded)
		{
			RowEvents = rowEvents?.ToImmutableArray() ?? ImmutableArray.Create<GraphRowEventInfo>(); 
		}

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}