#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacFieldGroupingNodeBaseViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		private readonly string _dacAndDacFieldNameForSearch;

		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public GraphEventCategoryNodeViewModel GraphEventsCategoryVM => DacVM.GraphEventsCategoryVM;

		public DacGroupingNodeBaseViewModel DacVM { get; }

		public string DacFieldName { get; }

		public override string Name
		{
			get => DacFieldName;
			protected set { }
		}

		public override Icon NodeIcon => Icon.GroupingDacField;

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => true;

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex { get; set; }

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.DisplayedChildren => DisplayedChildren;

		public ImmutableArray<GraphFieldEventInfo> FieldEvents { get; }

		protected DacFieldGroupingNodeBaseViewModel(DacGroupingNodeBaseViewModel dacVM, string dacFieldName, IEnumerable<GraphFieldEventInfo> dacFieldEvents,
													bool isExpanded) :
												base(dacVM?.Tree!, dacVM, isExpanded)
		{
			DacVM = dacVM!;
			DacFieldName = dacFieldName.CheckIfNullOrWhiteSpace();
			_dacAndDacFieldNameForSearch = $"{DacVM.DacName}#{dacFieldName}";
			FieldEvents = dacFieldEvents?.ToImmutableArray() ?? ImmutableArray.Create<GraphFieldEventInfo>();
		}

		public override bool NameMatchesPattern(string? pattern) => MatchPattern(_dacAndDacFieldNameForSearch, pattern);

		public async override Task NavigateToItemAsync()
		{
			var childToNavigateTo = this.GetChildToNavigateTo();

			if (childToNavigateTo != null)
			{
				await childToNavigateTo.NavigateToItemAsync();
				IsExpanded = true;
				Tree.SelectedItem = childToNavigateTo;
			}
		}

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => child is GraphMemberNodeViewModel;	
	}
}