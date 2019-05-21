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
	public class DacEventsGroupingNodeViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
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

		protected bool AllowNavigation => true;

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => AllowNavigation;

		protected int CurrentNavigationIndex
		{
			get;
			set;
		}

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex
		{
			get => CurrentNavigationIndex;
			set => CurrentNavigationIndex = value;
		}

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.Children => Children;

		protected DacEventsGroupingNodeViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM,
										   string dacName, bool isExpanded) :
									  base(graphEventsCategoryVM?.Tree, isExpanded)
		{
			dacName.ThrowOnNullOrWhiteSpace(nameof(dacName));

			GraphEventsCategoryVM = graphEventsCategoryVM;
			DacName = dacName;
		}

		public static DacEventsGroupingNodeViewModel Create(GraphEventCategoryNodeViewModel graphEventsCategoryVM,
															string dacName, IEnumerable<GraphRowEventInfo> graphEventsForDAC,
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

		protected virtual void FillDacNodeChildren(IEnumerable<GraphEventInfoBase> graphEventsForDAC, bool areChildrenExpanded)
		{
			var dacMembers = GraphEventsCategoryVM.CategoryType == GraphMemberType.FieldEvent
				? GetDacFieldEvents(graphEventsForDAC.OfType<GraphFieldEventInfo>(), areChildrenExpanded)
				: GetDacRowEvents(graphEventsForDAC.OfType<GraphRowEventInfo>(), areChildrenExpanded);

			Children.AddRange(dacMembers);
			EventsCount = GetDacNodeEventsCount();
			Children.CollectionChanged += DacChildrenChanged;
		}

		protected virtual IEnumerable<TreeNodeViewModel> GetDacRowEvents(IEnumerable<GraphRowEventInfo> graphEventsForDAC, bool areChildrenExpanded)
		{
			return graphEventsForDAC.Select(eventInfo => GraphEventsCategoryVM.CreateNewEventVM(this, eventInfo, areChildrenExpanded))
									.Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
									.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		protected virtual IEnumerable<TreeNodeViewModel> GetDacFieldEvents(IEnumerable<GraphFieldEventInfo> graphFieldEventsForDAC,
																		   bool areChildrenExpanded)
		{
			return from eventInfo in graphFieldEventsForDAC
				   group eventInfo by eventInfo.DacFieldName
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

		public override void NavigateToItem()
		{
			TreeNodeViewModel childToNavigateTo = null;

			switch (GraphEventsCategoryVM)
			{
				case CacheAttachedCategoryNodeViewModel cacheAttachedCategory:
				case RowEventCategoryNodeViewModel rowEventCategory:
					childToNavigateTo = this.GetChildToNavigateTo();
					break;
				case FieldEventCategoryNodeViewModel fieldEventCategory:
					childToNavigateTo = GetChildToNavigateToFromFieldEvents();
					  
					if (!(childToNavigateTo is FieldEventNodeViewModel fieldEventNode))
						return;

					fieldEventNode.DacFieldVM.IsExpanded = true;
					break;
			}


			if (childToNavigateTo != null)
			{				
				childToNavigateTo.NavigateToItem();
				IsExpanded = true;
				Tree.SelectedItem = childToNavigateTo;
			}
	}

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => CanNavigateToChild(child);

		protected bool CanNavigateToChild(TreeNodeViewModel child) =>
			child is RowEventNodeViewModel ||
			child is FieldEventNodeViewModel ||
			child is CacheAttachedNodeViewModel;

		protected TreeNodeViewModel GetChildToNavigateToFromFieldEvents()
		{
			if (AllowNavigation != true || Children.Count == 0)
				return null;
	
			List<TreeNodeViewModel> dacFieldEvents = Children.SelectMany(dacFieldEvent => dacFieldEvent.Children).ToList();

			if (dacFieldEvents.Count == 0)
				return null;

			int counter = 0;

			while (counter < EventsCount)
			{
				TreeNodeViewModel child = dacFieldEvents[CurrentNavigationIndex];
				CurrentNavigationIndex = (CurrentNavigationIndex + 1) % EventsCount;

				if (CanNavigateToChild(child))
				{
					return child;
				}

				counter++;
			}

			return null;
		}
	}
}