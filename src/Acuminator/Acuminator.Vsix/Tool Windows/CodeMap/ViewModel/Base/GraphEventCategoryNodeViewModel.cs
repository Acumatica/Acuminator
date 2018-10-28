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

		protected virtual GraphEventNodeByDacConstructor EventNodeByDacConstructor { get; } =
			(dacGroupVM, eventInfo) => new GraphMemberNodeViewModel(dacGroupVM.GraphMemberCategoryVM, eventInfo.Symbol);

		protected GraphEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												  bool isExpanded) : 
										   base(graphViewModel, graphMemberType, isExpanded)
		{
			_name = CategoryDescription;
		}

		protected override void AddCategoryMembers()
		{
			var graphSemanticModel = GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = GetCategoryGraphNodeSymbols()?.OfType<GraphEventInfo>();

			if (graphCategoryEvents.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from eventInfo in graphCategoryEvents.OrderBy(member => member.DeclarationOrder)
										where eventInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.GraphSymbol ||
											  eventInfo.Symbol.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.GraphSymbol.OriginalDefinition
										group eventInfo by eventInfo.DacName into dacFieldEvents
										select new DacGroupingNodeViewModel(this, dacFieldEvents, EventNodeByDacConstructor);

			Children.AddRange(graphMemberViewModels);
			int eventsCount = Children.Sum(node => node.Children.Count);
			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
