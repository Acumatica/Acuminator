using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacFieldEventsGroupingNodeViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventNodeViewModel(DacFieldEventsGroupingNodeViewModel dacFieldVM, GraphNodeSymbolItem memberInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, memberInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = memberInfo is GraphRowEventInfo eventInfo
				? eventInfo.EventType.ToString()
				: MemberSymbol.Name;
		}	
	}
}