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
	public class FieldEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
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

		public FieldEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
									base(graphViewModel, GraphMemberType.FieldEvent, isExpanded)
		{
			_name = CategoryDescription;
		}

		protected override void AddCategoryMembers()
		{
			var graphSemanticModel = GraphViewModel.GraphSemanticModel;
			var graphFieldEvents = graphSemanticModel.FieldDefaultingEvents
													 .Concat(graphSemanticModel.FieldVerifyingEvents)
													 .Concat(graphSemanticModel.FieldSelectingEvents)
													 .Concat(graphSemanticModel.FieldUpdatingEvents)
													 .Concat(graphSemanticModel.FieldUpdatedEvents)
													 .OrderBy(member => member.DeclarationOrder);

			var graphMemberViewModels = from eventInfo in graphFieldEvents
										where eventInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.GraphSymbol ||
											  eventInfo.Symbol.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.GraphSymbol.OriginalDefinition
										group eventInfo by eventInfo.DacName into dacFieldEvents
										select new DacGroupingNodeViewModel(this, dacFieldEvents,
															(dacGroupVM, eventInfo) =>
																	new GraphMemberNodeViewModel(dacGroupVM.GraphMemberCategoryVM,
																								 eventInfo.Symbol));
			Children.AddRange(graphMemberViewModels);
			int eventsCount = Children.Sum(node => node.Children.Count);
			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
