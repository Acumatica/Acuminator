using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ActionNodeViewModel : GraphMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.Action;

		public ActionInfo ActionInfo => MemberInfo as ActionInfo;

		public ActionNodeViewModel(ActionCategoryNodeViewModel actionCategoryVM, ActionInfo actionInfo, bool isExpanded = false) :
							  base(actionCategoryVM, actionCategoryVM, actionInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo IElementWithTooltip.CalculateTooltip()
		{
			string tooltip = ActionInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			return new TooltipInfo(tooltip);
		}
	}
}
