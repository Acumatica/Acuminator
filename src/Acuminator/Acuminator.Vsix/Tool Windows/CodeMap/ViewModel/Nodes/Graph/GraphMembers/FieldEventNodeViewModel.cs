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

		public FieldEventNodeViewModel(DacFieldEventsGroupingNodeViewModel dacFieldVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = eventInfo.EventType.ToString();
		}	
	}
}