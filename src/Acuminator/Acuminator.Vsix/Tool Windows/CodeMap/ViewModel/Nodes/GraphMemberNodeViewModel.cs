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

		public override string Name => MemberSymbol.Name;

		private GraphMemberNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, ISymbol memberSymbol, 
										 bool isExpanded) :
									base(graphMemberCategoryVM.Tree, isExpanded)
		{
			MemberSymbol = memberSymbol;
			MemberCategory = graphMemberCategoryVM;		
		}

		public static GraphMemberNodeViewModel Create(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, ISymbol memberSymbol,
													  bool isExpanded = true)
		{
			if (graphMemberCategoryVM == null || memberSymbol == null)
				return null;

			return new GraphMemberNodeViewModel(graphMemberCategoryVM, memberSymbol, isExpanded);
		}

		public override void NavigateToItem() => AcuminatorVSPackage.Instance.NavigateToSymbol(MemberSymbol);
	}
}
