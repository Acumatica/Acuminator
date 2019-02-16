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
	public class CacheAttachedNodeViewModel : GraphMemberNodeViewModel
	{
		public DacEventsGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public CacheAttachedNodeViewModel(DacEventsGroupingNodeViewModel dacViewModel, GraphNodeSymbolItem memberInfo,
										  bool isExpanded = false) :
									 base(dacViewModel?.GraphMemberCategoryVM, memberInfo, isExpanded)
		{
			DacViewModel = dacViewModel;

			int startPos = dacViewModel.DacName.Length + 1;
			int lastUnderscoreIndex = MemberSymbol.Name.LastIndexOf('_');
			Name = lastUnderscoreIndex > 0
				? MemberSymbol.Name.Substring(startPos, lastUnderscoreIndex - startPos)
				: MemberSymbol.Name;
			
			var attributeVMs = MemberSymbol.GetAttributes()
										   .Select(a => new AttributeNodeViewModel(this, a));
			Children.AddRange(attributeVMs);
		}	
	}
}
