#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class IsActiveGraphMethodNodeViewModel : IsActiveGraphMethodNodeViewModelBase
	{
		public IsActiveMethodInfo IsActiveMethodInfo => (IsActiveMethodInfo)MemberInfo;

		public IsActiveGraphMethodNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												IsActiveMethodInfo isActiveMethodInfo, bool isExpanded = false) :
										   base(graphInitializationAndActivationCategoryVM, isActiveMethodInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
