using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedNodeViewModel : GraphMemberNodeViewModel
	{
		public DacEventsGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public CacheAttachedNodeViewModel(DacEventsGroupingNodeViewModel dacViewModel, GraphFieldEventInfo eventInfo,
										  bool isExpanded = false) :
									 base(dacViewModel?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacViewModel = dacViewModel;
			Name = eventInfo.DacFieldName;			
			var attributeVMs = MemberSymbol.GetAttributes()
										   .Select(a => new AttributeNodeViewModel(this, a));
			Children.AddRange(attributeVMs);
		}	
	}
}
