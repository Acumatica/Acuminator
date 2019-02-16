using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphMemberCategoryNodeViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation
	{
		public GraphNodeViewModel GraphViewModel { get; }

		public PXGraphEventSemanticModel GraphSemanticModel => GraphViewModel.GraphSemanticModel;

		public GraphMemberType CategoryType { get; }

		protected string CategoryDescription { get; }

		public override string Name
		{
			get => $"{CategoryDescription}({Children.Count})";
			protected set { }
		}

		protected abstract bool AllowNavigation { get; }

		bool IGroupNodeWithCyclingNavigation.AllowNavigation { get; }

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex
		{
			get;
			set;
		}

		protected GraphMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												bool isExpanded) : 
										   base(graphViewModel?.Tree, isExpanded)
		{
			GraphViewModel = graphViewModel;
			CategoryType = graphMemberType;
			CategoryDescription = CategoryType.Description();
		}

		public override void NavigateToItem()
		{
			if (!AllowNavigation || Children.Count == 0)
				return;

			int counter = 0;
			TreeNodeViewModel childToNavigate = null;

			while (counter < Children.Count)
			{
				childToNavigate = Children[CurrentNavigationIndex] as GraphMemberNodeViewModel;

				if (childToNavigate != null)
				{
					CurrentNavigationIndex = (CurrentNavigationIndex + 1) % Children.Count;
					break;
				}

				counter++;
			}

			childToNavigate?.NavigateToItem();
		}

		public static GraphMemberCategoryNodeViewModel Create(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
															  bool isExpanded)
		{
			if (graphViewModel == null)
				return null;

			GraphMemberCategoryNodeViewModel memberCategoryVM = CreateCategory(graphViewModel, graphMemberType, isExpanded);
			memberCategoryVM?.AddCategoryMembers();
			return memberCategoryVM;
		}

		protected virtual void AddCategoryMembers()
		{
			IEnumerable<GraphNodeSymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

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

		protected abstract IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols();

		private static GraphMemberCategoryNodeViewModel CreateCategory(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
																		bool isExpanded)
		{
			switch (graphMemberType)
			{
				case GraphMemberType.View:
					return new ViewCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.Action:
					return new ActionCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.CacheAttached:
					return new CacheAttachedCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.RowEvent:
					return new RowEventCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.FieldEvent:
					return new FieldEventCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.NestedDAC:
				case GraphMemberType.NestedGraph:
				default:
					return null;
			}
		}

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) =>
			CanNavigateToChild(child);

		protected virtual bool CanNavigateToChild(TreeNodeViewModel child) => child is GraphMemberNodeViewModel;
	}
}
