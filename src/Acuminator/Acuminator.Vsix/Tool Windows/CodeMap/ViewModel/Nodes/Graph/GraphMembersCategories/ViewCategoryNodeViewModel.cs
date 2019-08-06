using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
		protected override bool AllowNavigation => true;

		public ViewCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.View, isExpanded)
		{
			
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.Views;

		protected override void AddCategoryMembers()
		{
			IEnumerable<SymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

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

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
