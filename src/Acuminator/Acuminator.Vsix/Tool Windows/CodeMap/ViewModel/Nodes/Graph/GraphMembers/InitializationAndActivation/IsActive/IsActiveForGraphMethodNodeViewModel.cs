#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class IsActiveForGraphMethodNodeViewModel : IsActiveGraphMethodNodeViewModelBase
	{
		public IsActiveForGraphMethodInfo IsActiveMethodForGraphInfo => (IsActiveForGraphMethodInfo)MemberInfo;

		public IsActiveForGraphMethodNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												   IsActiveForGraphMethodInfo isActiveMethodForGraphInfo, bool isExpanded = false) :
											  base(graphInitializationAndActivationCategoryVM, isActiveMethodForGraphInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
