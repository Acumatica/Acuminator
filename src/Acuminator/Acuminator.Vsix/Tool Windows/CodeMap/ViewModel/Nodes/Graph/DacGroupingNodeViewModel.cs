using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeViewModel : TreeNodeViewModel
	{
		public GraphMemberCategoryNodeViewModel GraphMemberCategoryVM { get; }

		public string DacName { get; }

		public override string Name
		{
			get => $"{DacName}({Children.Count})";
			protected set { }
		}

		public DacGroupingNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM,
										IGrouping<string, GraphEventInfo> graphEventsForDAC,
										GraphEventNodeByDacConstructor graphMemberCreator,
										bool isExpanded = false) :
								   base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			graphMemberCreator.ThrowOnNull(nameof(graphMemberCreator));

			if (graphEventsForDAC.IsNullOrEmpty())
			{
				throw new ArgumentNullException(nameof(graphEventsForDAC));
			}

			GraphMemberCategoryVM = graphMemberCategoryVM;
			DacName = graphEventsForDAC.Key;
			var graphMembers = graphEventsForDAC.Select(eventInfo => graphMemberCreator(this, eventInfo))
												.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
												.OrderBy(graphMemberVM => graphMemberVM.Name);
			Children.AddRange(graphMembers);
		}
	}
}
