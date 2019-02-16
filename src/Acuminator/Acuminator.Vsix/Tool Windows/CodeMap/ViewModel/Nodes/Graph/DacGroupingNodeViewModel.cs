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

		protected DacGroupingNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM,
										   string dacName, bool isExpanded = false) :
									  base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			dacName.ThrowOnNullOrWhiteSpace(nameof(dacName));

			GraphMemberCategoryVM = graphMemberCategoryVM;
			DacName = dacName;
		}

		public static DacGroupingNodeViewModel Create(GraphMemberCategoryNodeViewModel graphMemberCategoryVM,
													  string dacName, IEnumerable<GraphEventInfo> graphEventsForDAC,
													  GraphEventNodeByDacConstructor graphMemberCreator,
													  bool isDacExpanded = false, bool areChildrenExpanded = false)
		{
			if (graphMemberCreator == null || graphEventsForDAC.IsNullOrEmpty() || dacName.IsNullOrWhiteSpace())
			{
				return null;
			}

			DacGroupingNodeViewModel dacVM = new DacGroupingNodeViewModel(graphMemberCategoryVM, dacName, isDacExpanded);
			var dacMembers = dacVM.GetDacNodeChildren(graphEventsForDAC, graphMemberCreator, areChildrenExpanded);
			dacVM.Children.AddRange(dacMembers);
			return dacVM;
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacNodeChildren(IEnumerable<GraphEventInfo> graphEventsForDAC,
																				   GraphEventNodeByDacConstructor graphMemberCreator, 
																				   bool areChildrenExpanded)
		{
			if (GraphMemberCategoryVM.CategoryType == GraphMemberType.FieldEvent)
			{
				return GetDacFieldEvents(graphEventsForDAC, graphMemberCreator, areChildrenExpanded);
			}

			return GetDacEventsDefault(graphEventsForDAC, graphMemberCreator, areChildrenExpanded);
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacEventsDefault(IEnumerable<GraphEventInfo> graphEventsForDAC,
																					GraphEventNodeByDacConstructor graphMemberCreator,
																					bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => graphMemberCreator(this, eventInfo, areChildrenExpanded))
											.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
											.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacFieldEvents(IEnumerable<GraphEventInfo> graphEventsForDAC,
																				  GraphEventNodeByDacConstructor graphMemberCreator,
																				  bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => graphMemberCreator(this, eventInfo, areChildrenExpanded))
											.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
											.OrderBy(graphMemberVM => graphMemberVM.Name);
		}
	}
}
