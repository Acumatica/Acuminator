#nullable enable

using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class AuditDacFieldNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
											DacFieldInfo fieldInfo, ExpandCreatedNodesFunction isExpandedCalculator) : 
						DacFieldNodeViewModelBase(dacMemberCategoryVM, parent, fieldInfo, isExpandedCalculator)
	{
		public override Icon NodeIcon => Icon.DacAuditField;

		public override bool IconDependsOnCurrentTheme => true;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
