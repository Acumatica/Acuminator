using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeForFieldEventViewModel : DacGroupingNodeBaseViewModel
	{
		public ImmutableArray<GraphFieldEventInfo> AllFieldEvents { get; }

		public DacGroupingNodeForFieldEventViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName,
														IEnumerable<GraphFieldEventInfo> fieldEvents, bool isExpanded) :
												   base(graphEventsCategoryVM, dacName, isExpanded)
		{
			AllFieldEvents = fieldEvents?.ToImmutableArray() ?? ImmutableArray.Create<GraphFieldEventInfo>();
			ChildrenSortType = SortType.Alphabet;
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}