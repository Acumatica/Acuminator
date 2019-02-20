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
		public GraphEventCategoryNodeViewModel GraphEventsCategoryVM { get; }

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

		protected DacEventsGroupingNodeViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM,
										   string dacName, bool isExpanded) :
									  base(graphEventsCategoryVM?.Tree, isExpanded)
		{
			dacName.ThrowOnNullOrWhiteSpace(nameof(dacName));

			GraphEventsCategoryVM = graphEventsCategoryVM;
			DacName = dacName;
		}

		public static DacEventsGroupingNodeViewModel Create(GraphEventCategoryNodeViewModel graphEventsCategoryVM,
															string dacName, IEnumerable<GraphEventInfo> graphEventsForDAC,
															bool isDacExpanded = false, bool areChildrenExpanded = false)
		{
			if (graphEventsForDAC.IsNullOrEmpty() || dacName.IsNullOrWhiteSpace())
			{
				return null;
			}

			DacEventsGroupingNodeViewModel dacVM = new DacEventsGroupingNodeViewModel(graphEventsCategoryVM, dacName, isDacExpanded);
			dacVM.FillDacNodeChildren(graphEventsForDAC, areChildrenExpanded);
			return dacVM;
		}

		protected virtual void FillDacNodeChildren(IEnumerable<GraphEventInfo> graphEventsForDAC, bool areChildrenExpanded)
		{
			var dacMembers = GraphEventsCategoryVM.CategoryType == GraphMemberType.FieldEvent
				? GetDacFieldEvents(graphEventsForDAC, areChildrenExpanded)
				: GetDacEventsDefault(graphEventsForDAC, areChildrenExpanded);

			Children.AddRange(dacMembers);
			EventsCount = GetDacNodeEventsCount();
			Children.CollectionChanged += DacChildrenChanged;
		}

		protected virtual IEnumerable<TreeNodeViewModel> GetDacEventsDefault(IEnumerable<GraphEventInfo> graphEventsForDAC, bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => GraphEventsCategoryVM.CreateNewEventVM(this, eventInfo, areChildrenExpanded))
									.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
									.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		protected virtual IEnumerable<TreeNodeViewModel> GetDacFieldEvents(IEnumerable<GraphEventInfo> graphEventsForDAC, bool areChildrenExpanded)
		{
			return from eventInfo in graphEventsForDAC
				   group eventInfo by GraphEventInfo.GetDacFieldNameForFieldEvent(eventInfo) 
						into dacFieldEvents
				   select DacFieldEventsGroupingNodeViewModel.Create(this, dacFieldEvents.Key, dacFieldEvents) 
						into dacFieldNodeVM
				   where dacFieldNodeVM != null && !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   orderby dacFieldNodeVM.DacFieldName ascending
				   select dacFieldNodeVM;
		}

		protected virtual int GetDacNodeEventsCount() =>
			GraphEventsCategoryVM.CategoryType == GraphMemberType.FieldEvent
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
