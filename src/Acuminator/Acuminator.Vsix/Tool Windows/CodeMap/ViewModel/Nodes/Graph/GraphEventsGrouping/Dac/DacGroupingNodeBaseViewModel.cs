using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using System.Threading.Tasks;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacGroupingNodeBaseViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
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

		public override Icon NodeIcon => Icon.GroupingDac;

		public override bool DisplayNodeWithoutChildren => false;

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

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.Children => Children;

		protected DacGroupingNodeBaseViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName, bool isExpanded) :
												base(graphEventsCategoryVM?.Tree, graphEventsCategoryVM, isExpanded)
		{
			dacName.ThrowOnNullOrWhiteSpace(nameof(dacName));

			GraphEventsCategoryVM = graphEventsCategoryVM;
			DacName = dacName;

			Children.CollectionChanged += DacChildrenChanged;
		}

		protected virtual void DacChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			EventsCount = GraphEventsCategoryVM.CategoryType == GraphMemberType.FieldEvent
				? Children.Sum(dacFieldVM => dacFieldVM.Children.Count)
				: Children.Count; ;
		}

		public async override Task NavigateToItemAsync()
		{
			TreeNodeViewModel childToNavigateTo = null;

			switch (GraphEventsCategoryVM)
			{
				case CacheAttachedCategoryNodeViewModel _:
				case RowEventCategoryNodeViewModel _:
					childToNavigateTo = this.GetChildToNavigateTo();
					break;
				case FieldEventCategoryNodeViewModel _:
					childToNavigateTo = GetChildToNavigateToFromFieldEvents();

					if (!(childToNavigateTo is FieldEventNodeViewModel fieldEventNode))
						return;

					fieldEventNode.DacFieldVM.IsExpanded = true;
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