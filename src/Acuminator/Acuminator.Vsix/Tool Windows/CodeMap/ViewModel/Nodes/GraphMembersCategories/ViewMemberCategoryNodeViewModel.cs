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
	public class ViewMemberCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public ViewMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												bool isExpanded) : 
										   base(graphViewModel, graphMemberType, isExpanded)
		{
			
		}

		protected override void AddChildren()
		{
			var itemViewModels = from view in GraphViewModel.GraphSemanticModel.Views
								 select GraphMemberNodeViewModel.Create(this, view.Symbol, isExpanded: false) into itemViewModel
								 where itemViewModel != null
								 select itemViewModel;

			Children.AddRange(itemViewModels);
		}
	}
}
