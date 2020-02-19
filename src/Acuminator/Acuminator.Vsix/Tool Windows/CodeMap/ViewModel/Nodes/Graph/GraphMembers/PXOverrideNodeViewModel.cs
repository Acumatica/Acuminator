using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PXOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.PXOverride;

		public PXOverrideInfoForCodeMap PXOverrideInfo => MemberInfo as PXOverrideInfoForCodeMap;

		public PXOverrideNodeViewModel(PXOverridesCategoryNodeViewModel pxOverridesCategoryVM, 
									   PXOverrideInfoForCodeMap pxOverrideInfo, bool isExpanded = false) :
								  base(pxOverridesCategoryVM, pxOverrideInfo, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}
