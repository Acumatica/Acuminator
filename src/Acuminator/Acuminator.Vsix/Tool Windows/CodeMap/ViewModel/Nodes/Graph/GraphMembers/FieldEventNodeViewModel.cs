using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacFieldGroupingNodeViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventNodeViewModel(DacFieldGroupingNodeViewModel dacFieldVM, GraphNodeSymbolItem memberInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, memberInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = memberInfo is GraphEventInfo eventInfo
				? eventInfo.EventType.ToString()
				: MemberSymbol.Name;
		}	
	}
}