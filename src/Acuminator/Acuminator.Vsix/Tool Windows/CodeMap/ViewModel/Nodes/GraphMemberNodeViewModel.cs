using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberNodeViewModel : TreeNodeViewModel
	{
		public GraphMemberCategoryNodeViewModel MemberCategory { get; }

		public ISymbol MemberSymbol { get; }

		public GraphMemberType MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public GraphMemberNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, ISymbol memberSymbol, 
										 bool isExpanded = false) :
								   base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			memberSymbol.ThrowOnNull(nameof(memberSymbol));

			MemberSymbol = memberSymbol;
			MemberCategory = graphMemberCategoryVM;		
		}

		public override void NavigateToItem() => AcuminatorVSPackage.Instance.NavigateToSymbol(MemberSymbol);
	}
}
