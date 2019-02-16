using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



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
			Name = MemberInfo is GraphEventInfo eventInfo
				? eventInfo.EventType.ToString()
				: MemberSymbol.Name;
		}	
	}
}
