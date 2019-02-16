using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public FieldEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
										  base(graphViewModel, GraphMemberType.FieldEvent, isExpanded)
		{
		}

		protected override GraphEventNodeByDacConstructor EventNodeByDacConstructor { get; } =
			(dacGroupVM, eventInfo, isExpanded) => new FieldEventNodeViewModel(dacGroupVM, eventInfo, isExpanded);

		protected override IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.FieldDefaultingEvents
							  .Concat(GraphSemanticModel.FieldVerifyingEvents)
							  .Concat(GraphSemanticModel.FieldSelectingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatedEvents);
	}
}
