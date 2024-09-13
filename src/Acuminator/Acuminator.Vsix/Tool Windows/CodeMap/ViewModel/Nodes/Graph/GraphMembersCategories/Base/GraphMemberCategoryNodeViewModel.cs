#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphMemberCategoryNodeViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		public GraphNodeViewModel GraphViewModel { get; }

		public PXGraphEventSemanticModel GraphSemanticModel => GraphViewModel.GraphSemanticModel;

		public GraphSemanticModelForCodeMap CodeMapGraphModel => GraphViewModel.CodeMapGraphModel;

		public override bool DisplayNodeWithoutChildren => false;

		public GraphMemberType CategoryType { get; }

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

		protected GraphMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType, bool isExpanded) : 
										      base(graphViewModel?.Tree!, graphViewModel, isExpanded)
		{
			GraphViewModel = graphViewModel!;
			CategoryType = graphMemberType;
			CategoryDescription = CategoryType.Description();
		}

		public abstract IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols();

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

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) =>
			CanNavigateToChild(child);

		protected virtual bool CanNavigateToChild(TreeNodeViewModel child) => child is GraphMemberNodeViewModel;
	}
}