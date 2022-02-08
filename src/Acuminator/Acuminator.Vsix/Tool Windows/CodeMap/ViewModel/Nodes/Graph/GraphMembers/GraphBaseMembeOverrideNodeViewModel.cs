#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphBaseMembeOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public bool IsPersistMethodOverride => BaseMemberOverrideInfo.IsPersistMethodOverride;

		public override Icon NodeIcon => IsPersistMethodOverride
			? Icon.PersistMethodOverride
			: Icon.BaseMemberOverrideGraph;

		public BaseMemberOverrideInfo BaseMemberOverrideInfo => (BaseMemberOverrideInfo)MemberInfo;

		public GraphBaseMembeOverrideNodeViewModel(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategoryVM, 
												   BaseMemberOverrideInfo baseMemberOverrideInfo, bool isExpanded = false) :
											  base(graphBaseMemberOverridesCategoryVM, graphBaseMemberOverridesCategoryVM, baseMemberOverrideInfo, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
