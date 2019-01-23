using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberInfoNodeViewModel : TreeNodeViewModel
	{
		public GraphMemberCategoryNodeViewModel MemberCategory { get; }

		public GraphNodeSymbolItem MemberInfo { get; }

		public ISymbol MemberSymbol => MemberInfo.SymbolBase;

		public GraphMemberType MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public GraphMemberNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, GraphNodeSymbolItem memberInfo, 
										bool isExpanded = false) :
								   base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			memberInfo.ThrowOnNull(nameof(memberInfo));

			MemberInfo = memberInfo;
			MemberCategory = graphMemberCategoryVM;		
		}

		public override void NavigateToItem() => AcuminatorVSPackage.Instance.NavigateToSymbol(MemberSymbol);
	}
}
