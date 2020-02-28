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

		public override string Tooltip => ActionInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

		public override Icon NodeIcon => Icon.Action;

		public ActionNodeViewModel(ActionCategoryNodeViewModel actionCategoryVM, ActionInfo actionInfo, bool isExpanded = false) :
							  base(actionCategoryVM, actionInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
