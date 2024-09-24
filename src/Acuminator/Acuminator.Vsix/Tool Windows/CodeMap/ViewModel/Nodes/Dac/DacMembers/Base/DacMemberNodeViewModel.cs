#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacMemberNodeViewModel : TreeNodeViewModel, INodeWithDeclarationOrder
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public DacMemberCategoryNodeViewModel MemberCategory { get; }

		public SymbolItem MemberInfo { get; }

		public int DeclarationOrder => MemberInfo.DeclarationOrder;

		public ISymbol MemberSymbol => MemberInfo.SymbolBase;

		public DacMemberCategory MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public DacMemberNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
									  SymbolItem memberInfo, bool isExpanded) :
								 base(dacMemberCategoryVM?.Tree!, parent, isExpanded)
		{
			MemberInfo = memberInfo.CheckIfNull();
			MemberCategory = dacMemberCategoryVM!;
		}

		public override Task NavigateToItemAsync() =>
			TryNavigateToItemWithVisualStudioWorkspace(MemberSymbol)
				? Task.CompletedTask
				: MemberSymbol.NavigateToAsync();
	}
}
