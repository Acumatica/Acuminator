using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using System.Threading.Tasks;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberNodeViewModel : TreeNodeViewModel
	{
		public GraphMemberCategoryNodeViewModel MemberCategory { get; }

		public SymbolItem MemberInfo { get; }

		public ISymbol MemberSymbol => MemberInfo.SymbolBase;

		public GraphMemberType MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public override bool DisplayNodeWithoutChildren => true;

		public GraphMemberNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, SymbolItem memberInfo, 
										bool isExpanded = false) :
								   base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			memberInfo.ThrowOnNull(nameof(memberInfo));

			MemberInfo = memberInfo;
			MemberCategory = graphMemberCategoryVM;		
		}

		public override Task NavigateToItemAsync() => MemberSymbol.NavigateToAsync();
	}
}
