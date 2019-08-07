using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ActionNodeViewModel : GraphMemberNodeViewModel
	{
		public ActionInfo ActionInfo => MemberInfo as ActionInfo;

		public ActionNodeViewModel(ActionCategoryNodeViewModel actionCategoryVM, ActionInfo actionInfo, bool isExpanded = false) :
							  base(actionCategoryVM, actionInfo, isExpanded)
		{
		}	

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																	     CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
