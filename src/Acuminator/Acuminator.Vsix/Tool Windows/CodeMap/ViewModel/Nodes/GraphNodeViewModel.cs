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
	public class GraphNodeViewModel : TreeNodeViewModel
	{
		public PXGraphSemanticModel GraphSemanticModel { get; }
		public override string Name => GraphSemanticModel.Symbol.Name;

		private GraphNodeViewModel(PXGraphSemanticModel graphSemanticModel, TreeViewModel tree, bool isExpanded) : 
							 base(tree, isExpanded)
		{
			GraphSemanticModel = graphSemanticModel;
		}

		public static GraphNodeViewModel Create(PXGraphSemanticModel graphSemanticModel, TreeViewModel tree)
		{
			if (graphSemanticModel == null || tree == null)
				return null;

			GraphNodeViewModel graphNodeVM = new GraphNodeViewModel(graphSemanticModel, tree, isExpanded: false);
			graphNodeVM.AddGraphMemberCategories();
			return graphNodeVM;
		}

		private void AddGraphMemberCategories()
		{
			var memberCategories =
				GraphMemberTypeUtils.GetGraphMemberTypes()
									.Select(graphMemberType =>
										 GraphMemberCategoryNodeViewModel.Create(this, GraphMemberType.View, isExpanded: false));

			Children.AddRange(memberCategories);
		}
	}
}
