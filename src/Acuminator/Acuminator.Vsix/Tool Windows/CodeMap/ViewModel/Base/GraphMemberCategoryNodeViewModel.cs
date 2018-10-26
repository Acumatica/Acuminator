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
	public abstract class GraphMemberCategoryNodeViewModel : TreeNodeViewModel
	{
		public GraphNodeViewModel GraphViewModel { get; }

		public GraphMemberType CategoryType { get; }

		private readonly string _categoryDescription;

		public override string Name => $"{_categoryDescription}({Children.Count})";

		protected GraphMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												bool isExpanded) : 
										   base(graphViewModel?.Tree, isExpanded)
		{
			GraphViewModel = graphViewModel;
			CategoryType = graphMemberType;
			_categoryDescription = CategoryType.Description();
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

		protected abstract void AddCategoryMembers();

		protected void AddCategoryMembersDefaultImpl<T>(Func<PXGraphEventSemanticModel, IEnumerable<GraphNodeSymbolItem<T>>> graphMembersSelector)
		where T : ISymbol
		{
			if (graphMembersSelector == null)
				return;

			var itemViewModels = from graphMemberInfo in graphMembersSelector(GraphViewModel.GraphSemanticModel)
																	 .OrderBy(member => member.DeclarationOrder)
								 where graphMemberInfo.Symbol.ContainingType == GraphViewModel.GraphSemanticModel.GraphSymbol ||
									   graphMemberInfo.Symbol.ContainingType.OriginalDefinition == 
									   GraphViewModel.GraphSemanticModel.GraphSymbol.OriginalDefinition
								 select GraphMemberNodeViewModel.Create(this, graphMemberInfo.Symbol, isExpanded: false) into itemViewModel
								 where itemViewModel != null
								 select itemViewModel;

			Children.AddRange(itemViewModels);
		}

		private static GraphMemberCategoryNodeViewModel CreateCategory(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
																		bool isExpanded)
		{
			switch (graphMemberType)
			{
				case GraphMemberType.View:
					return new ViewCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.ViewDelegate:
					return new ViewDelegateCategoryNodeViewModel(graphViewModel, isExpanded);				
				case GraphMemberType.Action:
					return new ActionCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.ActionHandler:
					return new ActionHandlerCategoryNodeViewModel(graphViewModel, isExpanded);
				case GraphMemberType.CacheAttached:
				case GraphMemberType.RowEvent:
				case GraphMemberType.FieldEvent:
				case GraphMemberType.NestedDAC:
				case GraphMemberType.NestedGraph:
				default:
					return null;
			}
		}
	}
}
