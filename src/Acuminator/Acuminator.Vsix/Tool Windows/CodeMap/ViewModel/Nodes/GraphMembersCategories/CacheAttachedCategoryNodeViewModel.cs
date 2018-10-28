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
	public class CacheAttachedCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
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

		public CacheAttachedCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
									base(graphViewModel, GraphMemberType.CacheAttached, isExpanded)
		{
			_name = CategoryDescription;
		}

		protected override void AddCategoryMembers()
		{
			var cacheAttachedViewModels = from eventInfo in GraphViewModel.GraphSemanticModel
																				.CacheAttachedEvents
																				.OrderBy(member => member.DeclarationOrder)
										  where eventInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.GraphSymbol ||
												eventInfo.Symbol.ContainingType.OriginalDefinition ==
												GraphViewModel.GraphSemanticModel.GraphSymbol.OriginalDefinition
										  group eventInfo by eventInfo.DacName into dacEvents
										  select new DacGroupingNodeViewModel(this, dacEvents, 
																				(dacGroupVM, eventInfo) => 
																					new CacheAttachedNodeViewModel(dacGroupVM, eventInfo.Symbol));		
			Children.AddRange(cacheAttachedViewModels);
			int eventsCount = Children.Sum(node => node.Children.Count);
			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
