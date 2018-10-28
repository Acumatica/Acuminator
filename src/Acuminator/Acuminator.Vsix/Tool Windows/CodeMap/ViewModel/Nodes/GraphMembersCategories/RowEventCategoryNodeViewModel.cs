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
	public class RowEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		private string _name;

		public override string Name
		{
			get => _name;
			protected set
			{
				if (_name != value)
				{
					_name = value;
					NotifyPropertyChanged();
				}
			}
		}

		public RowEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.RowEvent, isExpanded)
		{
			_name = CategoryDescription;
		}

		protected override void AddCategoryMembers()
		{ 
			var graphSemanticModel = GraphViewModel.GraphSemanticModel;
			var graphRowEvents = graphSemanticModel.RowInsertingEvents
					                               .Concat(graphSemanticModel.RowInsertedEvents)
					                               .Concat(graphSemanticModel.RowSelectingEvents)
					                               .Concat(graphSemanticModel.RowSelectedEvents)
					                               .Concat(graphSemanticModel.RowUpdatingEvents)
					                               .Concat(graphSemanticModel.RowUpdatedEvents)
					                               .Concat(graphSemanticModel.RowDeletingEvents)
					                               .Concat(graphSemanticModel.RowDeletedEvents)
					                               .Concat(graphSemanticModel.RowPersistingEvents)
					                               .Concat(graphSemanticModel.RowPersistedEvents);

			var graphMemberViewModels = from eventInfo in graphRowEvents
										where eventInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.GraphSymbol ||
											  eventInfo.Symbol.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.GraphSymbol.OriginalDefinition
										group eventInfo by eventInfo.DacName into dacRowEvents
										select new DacGroupingNodeViewModel(this, dacRowEvents,
															(dacGroupVM, eventInfo) =>
																	new GraphMemberNodeViewModel(dacGroupVM.GraphMemberCategoryVM,
																								 eventInfo.Symbol));
			Children.AddRange(graphMemberViewModels);
			int eventsCount = Children.Sum(node => node.Children.Count);
			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
