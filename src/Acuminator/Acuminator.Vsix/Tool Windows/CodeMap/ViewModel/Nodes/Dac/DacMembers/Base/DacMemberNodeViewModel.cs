#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;

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

		public DacMemberNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
									  SymbolItem memberInfo, bool isExpanded = false) :
								 base(dacMemberCategoryVM?.Tree!, parent, isExpanded)
		{
			MemberInfo = memberInfo.CheckIfNull();
			MemberCategory = dacMemberCategoryVM!;
		}

		public override Task NavigateToItemAsync() => MemberSymbol.NavigateToAsync();
	}
}
