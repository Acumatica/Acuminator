using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacEventsGroupingNodeViewModel : TreeNodeViewModel
	{
		public GraphEventCategoryNodeViewModel GraphMemberCategoryVM { get; }

		public string DacName { get; }

		public int EventsCount
		{
			get;
			protected set;
		}

		public override string Name
		{
			get => $"{DacName}({EventsCount})";
			protected set { }
		}

		protected DacEventsGroupingNodeViewModel(GraphMemberCategoryNodeViewModel graphMemberCategoryVM,
										   string dacName, bool isExpanded) :
									  base(graphMemberCategoryVM?.Tree, isExpanded)
		{
			dacName.ThrowOnNullOrWhiteSpace(nameof(dacName));

			GraphMemberCategoryVM = graphMemberCategoryVM;
			DacName = dacName;
		}

		public static DacEventsGroupingNodeViewModel Create(GraphMemberCategoryNodeViewModel graphMemberCategoryVM,
													  string dacName, IEnumerable<GraphEventInfo> graphEventsForDAC,
													  bool isDacExpanded = false, bool areChildrenExpanded = false)
		{
			if (graphEventsForDAC.IsNullOrEmpty() || dacName.IsNullOrWhiteSpace())
			{
				return null;
			}

			DacEventsGroupingNodeViewModel dacVM = new DacEventsGroupingNodeViewModel(graphMemberCategoryVM, dacName, isDacExpanded);
			dacVM.FillDacNodeChildren(graphEventsForDAC, areChildrenExpanded);
			return dacVM;
		}

		protected virtual void FillDacNodeChildren(IEnumerable<GraphEventInfo> graphEventsForDAC, bool areChildrenExpanded)
		{
			var dacMembers = GraphMemberCategoryVM.CategoryType == GraphMemberType.FieldEvent
				? GetDacFieldEvents(graphEventsForDAC, areChildrenExpanded)
				: GetDacEventsDefault(graphEventsForDAC, areChildrenExpanded);

			Children.AddRange(dacMembers);
			EventsCount = GetDacNodeEventsCount();
			Children.CollectionChanged += DacChildrenChanged;
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacEventsDefault(IEnumerable<GraphEventInfo> graphEventsForDAC,
																					bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => GraphMemberCategoryVM.Crea(this, eventInfo, areChildrenExpanded))
									.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
									.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacFieldEvents(IEnumerable<GraphEventInfo> graphEventsForDAC,
																				  bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => graphMemberCreator(this, eventInfo, areChildrenExpanded))
											.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
											.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		protected virtual int GetDacNodeEventsCount() =>
			GraphMemberCategoryVM.CategoryType == GraphMemberType.FieldEvent
				? Children.Sum(dacFieldVM => dacFieldVM.Children.Count)
				: Children.Count;

		protected virtual void DacChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			EventsCount = GetDacNodeEventsCount();
		}
	}
}
