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
	public class RowEventCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public RowEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
										base(graphViewModel, GraphMemberType.RowEvent, isExpanded)
		{
		}

		protected override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.RowInsertingEvents
							  .Concat(GraphSemanticModel.RowInsertedEvents)
							  .Concat(GraphSemanticModel.RowSelectingEvents)
							  .Concat(GraphSemanticModel.RowSelectedEvents)
							  .Concat(GraphSemanticModel.RowUpdatingEvents)
							  .Concat(GraphSemanticModel.RowUpdatedEvents)
							  .Concat(GraphSemanticModel.RowDeletingEvents)
							  .Concat(GraphSemanticModel.RowDeletedEvents)
							  .Concat(GraphSemanticModel.RowPersistingEvents)
							  .Concat(GraphSemanticModel.RowPersistedEvents);

		public override IEnumerable<TreeNodeViewModel> GetEventsViewModelsForDAC(DacEventsGroupingNodeViewModel dacVM, 
																				 IEnumerable<GraphEventInfoBase> graphEventsForDAC,
																				 bool areChildrenExpanded)
		{
			return graphEventsForDAC.OfType<GraphRowEventInfo>()
									.Select(eventInfo => CreateNewEventVM(dacVM, eventInfo, areChildrenExpanded))
									.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
									.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		public override GraphMemberNodeViewModel CreateNewEventVM<TEventNodeParent>(TEventNodeParent eventNodeParent, GraphEventInfoBase eventInfo,
																					bool isExpanded)
		{
			return eventNodeParent is DacEventsGroupingNodeViewModel dacGroupVM && eventInfo is GraphRowEventInfo rowEventInfo
				? new RowEventNodeViewModel(dacGroupVM, rowEventInfo, isExpanded)
				: base.CreateNewEventVM(eventNodeParent, eventInfo, isExpanded);
		}
	}
}
