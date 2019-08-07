using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeForCacheAttachedEventViewModel : DacFieldGroupingNodeBaseViewModel
	{
		public DacFieldGroupingNodeForCacheAttachedEventViewModel(DacGroupingNodeBaseViewModel dacVM, string dacFieldName,
																  IEnumerable<GraphFieldEventInfo> dacFieldEvents, bool isExpanded) :
															 base(dacVM, dacFieldName, dacFieldEvents, isExpanded)
		{
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}