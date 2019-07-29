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

		protected override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.FieldDefaultingEvents
							  .Concat(GraphSemanticModel.FieldVerifyingEvents)
							  .Concat(GraphSemanticModel.FieldSelectingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatedEvents)
							  .Concat(GraphSemanticModel.ExceptionHandlingEvents)
							  .Concat(GraphSemanticModel.CommandPreparingEvents);

		public override IEnumerable<TreeNodeViewModel> GetEventsViewModelsForDAC(DacEventsGroupingNodeViewModel dacVM,
																				 IEnumerable<GraphEventInfoBase> graphFieldEventsForDAC,
																				 bool areChildrenExpanded)
		{
			return from eventInfo in graphFieldEventsForDAC.OfType<GraphFieldEventInfo>()
				   group eventInfo by eventInfo.DacFieldName
						into dacFieldEvents
				   select DacFieldEventsGroupingNodeViewModel.Create(dacVM, dacFieldEvents.Key, dacFieldEvents)
						into dacFieldNodeVM
				   where dacFieldNodeVM != null && !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   orderby dacFieldNodeVM.DacFieldName ascending
				   select dacFieldNodeVM;
		}

		public override GraphMemberNodeViewModel CreateNewEventVM<TEventNodeParent>(TEventNodeParent eventNodeParent, GraphEventInfoBase eventInfo,
																					bool isExpanded)
		{
			if (eventNodeParent is DacFieldEventsGroupingNodeViewModel dacFieldVM && eventInfo is GraphFieldEventInfo fieldEventInfo)
				return new FieldEventNodeViewModel(dacFieldVM, fieldEventInfo, isExpanded);		
			else
				return base.CreateNewEventVM(eventNodeParent, eventInfo, isExpanded);
		}
	}
}
