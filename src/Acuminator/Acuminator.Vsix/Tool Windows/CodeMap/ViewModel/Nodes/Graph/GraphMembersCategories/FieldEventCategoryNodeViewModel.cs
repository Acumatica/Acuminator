using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public FieldEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
										  base(graphViewModel, GraphMemberType.FieldEvent, isExpanded)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.FieldDefaultingEvents
							  .Concat(GraphSemanticModel.FieldVerifyingEvents)
							  .Concat(GraphSemanticModel.FieldSelectingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatedEvents)
							  .Concat(GraphSemanticModel.ExceptionHandlingEvents)
							  .Concat(GraphSemanticModel.CommandPreparingEvents);

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
