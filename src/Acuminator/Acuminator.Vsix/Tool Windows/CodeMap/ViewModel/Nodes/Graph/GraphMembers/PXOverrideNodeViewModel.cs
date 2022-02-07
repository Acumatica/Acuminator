using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PXOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.PXOverride;

		public PXOverrideInfo PXOverrideInfo => MemberInfo as PXOverrideInfo;

		public PXOverrideNodeViewModel(PXOverridesCategoryNodeViewModel pxOverridesCategoryVM, 
									   PXOverrideInfo pxOverrideInfo, bool isExpanded = false) :
								  base(pxOverridesCategoryVM, pxOverridesCategoryVM, pxOverrideInfo, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
