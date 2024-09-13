#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacMemberCategoryNodeViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		public DacNodeViewModel DacViewModel { get; }

		public DacSemanticModel DacModel => DacViewModel.DacModel;

		public override bool DisplayNodeWithoutChildren => false;

		public DacMemberCategory CategoryType { get; }

		protected string CategoryDescription { get; }

		public override string Name
		{
			get => $"{CategoryDescription}({DisplayedChildren.Count})";
			protected set { }
		}

		public override Icon NodeIcon => Icon.Category;

		protected abstract bool AllowNavigation { get; }

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => AllowNavigation;

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex
		{
			get;
			set;
		}

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.DisplayedChildren => DisplayedChildren;

		protected DacMemberCategoryNodeViewModel(DacNodeViewModel dacViewModel, DacMemberCategory dacCategoryType, bool isExpanded) : 
										    base(dacViewModel?.Tree!, dacViewModel, isExpanded)
		{
			DacViewModel = dacViewModel!;
			CategoryType = dacCategoryType;
			CategoryDescription = CategoryType.Description();
		}

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

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => CanNavigateToChild(child);

		protected virtual bool CanNavigateToChild(TreeNodeViewModel child) => true;
	}
}