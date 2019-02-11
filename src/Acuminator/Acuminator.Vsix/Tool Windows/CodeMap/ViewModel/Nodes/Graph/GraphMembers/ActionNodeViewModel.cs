using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ActionNodeViewModel : GraphMemberNodeViewModel
	{
		public ActionNodeViewModel(ActionCategoryNodeViewModel actionCategoryVM, ActionInfo actionInfo, bool isExpanded = false) :
							  base(actionCategoryVM, actionInfo, isExpanded)
		{
			AddActionHandler();
		}	

		protected virtual void AddActionHandler()
		{
			if (MemberCategory.GraphSemanticModel.ActionHandlersByNames.TryGetValue(MemberSymbol.Name, out ActionHandlerInfo actionHandler))
			{
				Children.Add(new GraphMemberInfoNodeViewModel(this, actionHandler, GraphMemberInfoType.ActionHandler));
			}
		}
	}
}
