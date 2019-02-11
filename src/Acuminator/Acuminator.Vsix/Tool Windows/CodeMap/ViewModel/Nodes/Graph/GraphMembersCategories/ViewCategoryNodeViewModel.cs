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
	/// <summary>
	/// A ViewModel for the view category CodeMap node.
	/// </summary>
	public class ViewCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public ViewCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.View, isExpanded)
		{
			
		}

		protected override IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.Views;

		protected override void AddCategoryMembers()
		{
			IEnumerable<GraphNodeSymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

			if (categoryTreeNodes.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from viewInfo in categoryTreeNodes.OfType<DataViewInfo>()
										where viewInfo.SymbolBase.ContainingType == GraphViewModel.GraphSemanticModel.Symbol ||
											  viewInfo.SymbolBase.ContainingType.OriginalDefinition ==
											  GraphViewModel.GraphSemanticModel.Symbol.OriginalDefinition
										orderby viewInfo.SymbolBase.Name
										select new ViewNodeViewModel(this, viewInfo);

			Children.AddRange(graphMemberViewModels);
		}
	}
}
