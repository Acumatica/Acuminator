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
	public class ActionHandlerCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public ActionHandlerCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
											 base(graphViewModel, GraphMemberType.ActionHandler, isExpanded)
		{		
		}

		protected override IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.ActionHandlers;
	}
}
