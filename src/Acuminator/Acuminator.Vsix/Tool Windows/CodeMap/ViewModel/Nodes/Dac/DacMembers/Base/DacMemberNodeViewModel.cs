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
	public abstract class DacMemberNodeViewModel : TreeNodeViewModel, INodeWithSymbolItem
	{
		public DacMemberCategoryNodeViewModel MemberCategory { get; }

		public SymbolItem MemberInfo { get; }

		SymbolItem INodeWithSymbolItem.Symbol => MemberInfo;

		public ISymbol MemberSymbol => MemberInfo.SymbolBase;

		public DacMemberCategory MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public override bool DisplayNodeWithoutChildren => true;

		public DacMemberNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, SymbolItem memberInfo, bool isExpanded = false) :
								 base(dacMemberCategoryVM?.Tree, isExpanded)
		{
			memberInfo.ThrowOnNull(nameof(memberInfo));

			MemberInfo = memberInfo;
			MemberCategory = dacMemberCategoryVM;		
		}

		public override Task NavigateToItemAsync() => MemberSymbol.NavigateToAsync();
	}
}
