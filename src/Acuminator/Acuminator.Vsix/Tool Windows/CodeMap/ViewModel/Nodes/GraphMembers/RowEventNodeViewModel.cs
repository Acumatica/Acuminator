using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public RowEventNodeViewModel(DacGroupingNodeViewModel dacViewModel, GraphNodeSymbolItem memberInfo, bool isExpanded = false) :
								base(dacViewModel?.GraphMemberCategoryVM, memberInfo, isExpanded)
		{
			DacViewModel = dacViewModel;

			int startPos = dacViewModel.DacName.Length + 1;
			int lastUnderscoreIndex = MemberSymbol.Name.LastIndexOf('_');
			Name = lastUnderscoreIndex > 0
				? MemberSymbol.Name.Substring(startPos)
				: MemberSymbol.Name;
		}	
	}
}
