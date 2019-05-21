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
	public class ActionCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public ActionCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									  base(graphViewModel, GraphMemberType.Action, isExpanded)
		{
			
		}

		protected override IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.Actions;

		protected override void AddCategoryMembers()
		{
			IEnumerable<GraphNodeSymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

			if (categoryTreeNodes.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from actionInfo in categoryTreeNodes.OfType<ActionInfo>()
										where actionInfo.SymbolBase.ContainingType == GraphViewModel.GraphSemanticModel.Symbol ||
											  actionInfo.SymbolBase.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.Symbol.OriginalDefinition
										orderby actionInfo.SymbolBase.Name
										select new ActionNodeViewModel(this, actionInfo);

			Children.AddRange(graphMemberViewModels);
		}
	}
}
