using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ActionNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.Action;

		public ActionInfo ActionInfo => MemberInfo as ActionInfo;

		public override string Tooltip => ActionInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

		public ActionNodeViewModel(ActionCategoryNodeViewModel actionCategoryVM, ActionInfo actionInfo, bool isExpanded = false) :
							  base(actionCategoryVM, actionCategoryVM, actionInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
