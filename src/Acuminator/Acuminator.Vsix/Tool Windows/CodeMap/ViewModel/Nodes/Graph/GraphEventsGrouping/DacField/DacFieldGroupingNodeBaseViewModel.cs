using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacFieldGroupingNodeBaseViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		public GraphEventCategoryNodeViewModel GraphEventsCategoryVM => DacVM.GraphEventsCategoryVM;

		public DacGroupingNodeBaseViewModel DacVM { get; }

		public string DacFieldName { get; }

		public override string Name
		{
			get => DacFieldName;
			protected set { }
		}

		public override Icon NodeIcon => Icon.GroupingDacField;

		public override bool DisplayNodeWithoutChildren => false;

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => true;

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex { get; set; }

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.Children => Children;

		public ImmutableArray<GraphFieldEventInfo> FieldEvents { get; }

		protected DacFieldGroupingNodeBaseViewModel(DacGroupingNodeBaseViewModel dacVM, string dacFieldName, IEnumerable<GraphFieldEventInfo> dacFieldEvents,
													bool isExpanded) :
											  base(dacVM?.Tree, isExpanded)
		{
			dacFieldName.ThrowOnNullOrWhiteSpace(nameof(dacFieldName));

			DacVM = dacVM;
			DacFieldName = dacFieldName;
			FieldEvents = dacFieldEvents?.ToImmutableArray() ?? ImmutableArray.Create<GraphFieldEventInfo>();
		}

		public override bool IsSortTypeSupported(SortType sortType) => sortType == SortType.Alphabet;

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