using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeForRowEventHandlerViewModel : DacGroupingNodeBaseViewModel
	{
		public ImmutableArray<GraphRowEventHandlerInfo> RowEventHandlers { get; }

		public DacGroupingNodeForRowEventHandlerViewModel(GraphEventHandlerCategoryNodeViewModel graphEventHandlersCategoryVM, string dacName,
														  IEnumerable<GraphRowEventHandlerInfo> rowEventHandlers,
														  ExpandCreatedNodesFunction isExpandedCalculator) :
													 base(graphEventHandlersCategoryVM, dacName, isExpandedCalculator)
		{
			RowEventHandlers = rowEventHandlers?.ToImmutableArray() ?? ImmutableArray.Create<GraphRowEventHandlerInfo>(); 
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}