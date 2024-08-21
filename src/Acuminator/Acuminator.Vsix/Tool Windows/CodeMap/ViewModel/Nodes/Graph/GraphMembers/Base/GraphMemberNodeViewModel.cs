﻿#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphMemberNodeViewModel : TreeNodeViewModel, INodeWithSymbolItem
	{
		public GraphMemberCategoryNodeViewModel MemberCategory { get; }

		public SymbolItem MemberInfo { get; }

		SymbolItem INodeWithSymbolItem.Symbol => MemberInfo;

		public ISymbol MemberSymbol => MemberInfo.SymbolBase;

		public GraphMemberType MemberType => MemberCategory.CategoryType;

		public override string Name
		{
			get => MemberSymbol.Name;
			protected set { }
		}

		public override bool DisplayNodeWithoutChildren => true;

		public GraphMemberNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM, TreeNodeViewModel parent, SymbolItem memberInfo, 
										bool isExpanded = false) :
								   base(graphMemberCategoryVM?.Tree!, parent, isExpanded)
		{
			MemberInfo = memberInfo.CheckIfNull();
			MemberCategory = graphMemberCategoryVM!;
		}

		public override Task NavigateToItemAsync() => MemberSymbol.NavigateToAsync();
	}
}
