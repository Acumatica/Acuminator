using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PXOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public PXOverrideInfoForCodeMap PXOverrideInfo => MemberInfo as PXOverrideInfoForCodeMap;

		public PXOverrideNodeViewModel(PXOverridesCategoryNodeViewModel pxOverridesCategoryVM, 
									   PXOverrideInfoForCodeMap pxOverrideInfo, bool isExpanded = false) :
								  base(pxOverridesCategoryVM, pxOverrideInfo, isExpanded)
		{		
		}	
	}
}
