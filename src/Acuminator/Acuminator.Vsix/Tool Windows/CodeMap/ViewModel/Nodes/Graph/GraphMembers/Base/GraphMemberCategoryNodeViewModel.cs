using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using System.Threading.Tasks;
using System.Threading;

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
			get => $"{CategoryDescription}({Children.Count})";
			protected set { }
		}

		protected abstract bool AllowNavigation { get; }

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => AllowNavigation;

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex
		{
			get;
			set;
		}

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.Children => Children;

		protected GraphMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												bool isExpanded) : 
										   base(graphViewModel?.Tree, isExpanded)
		{
			GraphViewModel = graphViewModel;
			CategoryType = graphMemberType;
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

		protected virtual void AddCategoryMembers()
		{
			IEnumerable<SymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

			if (categoryTreeNodes.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from graphMemberInfo in categoryTreeNodes
										where graphMemberInfo.SymbolBase.ContainingType == GraphViewModel.GraphSemanticModel.Symbol ||
											  graphMemberInfo.SymbolBase.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.Symbol.OriginalDefinition
										orderby graphMemberInfo.SymbolBase.Name
										select new GraphMemberNodeViewModel(this, graphMemberInfo);

			Children.AddRange(graphMemberViewModels);
		}

		public abstract IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols();

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) =>
			CanNavigateToChild(child);

		protected virtual bool CanNavigateToChild(TreeNodeViewModel child) => child is GraphMemberNodeViewModel;
	}
}