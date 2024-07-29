#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacBqlFieldNodeViewModel : DacMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.DacBqlField;

		public DacFieldInfo FieldInfo => (MemberInfo as DacFieldInfo)!;

		public DacBqlFieldNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent,
										DacFieldInfo fieldInfo, bool isExpanded = false) :
										base(dacMemberCategoryVM, parent, fieldInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
