using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
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

		protected override bool AllowNavigation => false;

		protected GraphEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												  bool isExpanded) :
										   base(graphViewModel, graphMemberType, isExpanded)
		{
			_name = CategoryDescription;
		}

		protected override void AddCategoryMembers()
		{
			var graphSemanticModel = GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = GetCategoryGraphNodeSymbols()?.OfType<GraphRowEventInfo>()
																	.Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEvents.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from eventInfo in graphCategoryEvents
										where eventInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.Symbol ||
											  eventInfo.Symbol.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.Symbol.OriginalDefinition
										group eventInfo by eventInfo.DacName into graphEventsForDAC
										select DacEventsGroupingNodeViewModel.Create(this, graphEventsForDAC.Key, graphEventsForDAC) into dacNodeVM
										where dacNodeVM != null
										orderby dacNodeVM.DacName ascending
										select dacNodeVM;

			Children.AddRange(graphMemberViewModels);

			int eventsCount = Children.OfType<DacEventsGroupingNodeViewModel>().Sum(dacVM => dacVM.EventsCount);
			Name = $"{CategoryDescription}({eventsCount})";
		}

		public virtual GraphMemberNodeViewModel CreateNewEventVM<TEventNodeParent>(TEventNodeParent eventNodeParent, GraphEventInfoBase eventInfo,
																				   bool isExpanded)
		where TEventNodeParent : TreeNodeViewModel
		{
			return new GraphMemberNodeViewModel(this, eventInfo, isExpanded);
		}
	}
}
