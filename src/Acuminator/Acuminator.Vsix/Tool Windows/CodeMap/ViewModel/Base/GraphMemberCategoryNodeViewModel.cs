using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

		public override string Name { get; }

		protected GraphMemberCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
												bool isExpanded) : 
										   base(graphViewModel?.Tree, isExpanded)
		{
			GraphViewModel = graphViewModel;
			CategoryType = graphMemberType;
			Name = CategoryType.Description();
		}

		public static GraphMemberCategoryNodeViewModel Create(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
															  bool isExpanded)
		{
			if (graphViewModel == null)
				return null;

			GraphMemberCategoryNodeViewModel memberCategoryVM = CreateCategory(graphViewModel, graphMemberType, isExpanded);
			memberCategoryVM?.AddChildren();
			return memberCategoryVM;
		}

		protected abstract void AddChildren();

		private static GraphMemberCategoryNodeViewModel CreateCategory(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType,
																		bool isExpanded)
		{
			switch (graphMemberType)
			{
				case GraphMemberType.View:
					return new ViewMemberCategoryNodeViewModel(graphViewModel, graphMemberType, isExpanded);
				case GraphMemberType.ViewDelegate:
					break;
				case GraphMemberType.Action:
					break;
				case GraphMemberType.ActionHandler:
					break;
				case GraphMemberType.CacheAttached:
					break;
				case GraphMemberType.RowEvent:
					break;
				case GraphMemberType.FieldEvent:
					break;
				case GraphMemberType.NestedDAC:
					break;
				case GraphMemberType.NestedGraph:
					break;
				default:
					return null;
			}
		}
	}
}
