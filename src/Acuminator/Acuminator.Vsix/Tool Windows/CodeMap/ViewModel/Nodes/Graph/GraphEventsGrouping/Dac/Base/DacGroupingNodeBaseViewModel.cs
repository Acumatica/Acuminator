#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacGroupingNodeBaseViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfChildrenMeetFilter;

		public GraphEventHandlerCategoryNodeViewModel GraphEventHandlersCategoryVM { get; }

		public string DacName { get; }

		public int EventHandlersCount
		{
			get;
			protected set;
		}

		public override string Name
		{
			get => $"{DacName}({EventHandlersCount})";
			protected set { }
		}

		public override Icon NodeIcon => Icon.GroupingDac;

		protected virtual bool AllowNavigation => true;

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

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.DisplayedChildren => DisplayedChildren;

		protected DacGroupingNodeBaseViewModel(GraphEventHandlerCategoryNodeViewModel graphEventHandlersCategoryVM, string dacName,
											   ExpandCreatedNodesFunction isExpandedCalculator) :
										  base(graphEventHandlersCategoryVM?.Tree!, graphEventHandlersCategoryVM, isExpandedCalculator)
		{
			dacName.ThrowOnNullOrWhiteSpace();

			GraphEventHandlersCategoryVM = graphEventHandlersCategoryVM!;
			DacName = dacName;

			SubscribeOnDisplayedChildrenCollectionChanged(DacChildrenChanged);
		}

		protected virtual void DacChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			EventHandlersCount = GraphEventHandlersCategoryVM.CategoryType == GraphMemberCategory.FieldEvent
				? DisplayedChildren.Sum(dacFieldVM => dacFieldVM.DisplayedChildren.Count)
				: DisplayedChildren.Count;
		}

		public async override Task NavigateToItemAsync()
		{
			TreeNodeViewModel? childToNavigateTo = null;

			switch (GraphEventHandlersCategoryVM)
			{
				case CacheAttachedCategoryNodeViewModel _:
				case RowEventHandlerCategoryNodeViewModel _:
					childToNavigateTo = this.GetChildToNavigateTo();
					break;
				case FieldEventHandlerCategoryNodeViewModel _:
					childToNavigateTo = GetChildToNavigateToFromFieldEvents();

					if (childToNavigateTo is not FieldEventHandlerNodeViewModel fieldEventHandlerNode)
						return;

					fieldEventHandlerNode.DacFieldVM.IsExpanded = true;
					break;
			}


			if (childToNavigateTo != null)
			{
				await childToNavigateTo.NavigateToItemAsync();
				IsExpanded = true;
				Tree.SelectedItem = childToNavigateTo;
			}
		}

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => CanNavigateToChild(child);

		protected bool CanNavigateToChild(TreeNodeViewModel child) =>
			child is RowEventHandlerNodeViewModel ||
			child is FieldEventHandlerNodeViewModel ||
			child is CacheAttachedNodeViewModel;

		protected TreeNodeViewModel? GetChildToNavigateToFromFieldEvents()
		{
			if (AllowNavigation != true || DisplayedChildren.Count == 0)
				return null;

			List<TreeNodeViewModel> dacFieldEventHandlers = DisplayedChildren.SelectMany(dacFieldEventHandler => dacFieldEventHandler.DisplayedChildren).ToList();

			if (dacFieldEventHandlers.Count == 0)
				return null;

			int counter = 0;

			while (counter < EventHandlersCount)
			{
				TreeNodeViewModel child = dacFieldEventHandlers[CurrentNavigationIndex];
				CurrentNavigationIndex = (CurrentNavigationIndex + 1) % EventHandlersCount;

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