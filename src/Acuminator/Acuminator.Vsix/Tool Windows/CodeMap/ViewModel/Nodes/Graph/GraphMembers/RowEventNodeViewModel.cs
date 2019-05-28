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
		public DacEventsGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public RowEventNodeViewModel(DacEventsGroupingNodeViewModel dacViewModel, GraphRowEventInfo eventInfo, bool isExpanded = false) :
								base(dacViewModel?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacViewModel = dacViewModel;
			Name = eventInfo.EventType.ToString();
		}	
	}
}
