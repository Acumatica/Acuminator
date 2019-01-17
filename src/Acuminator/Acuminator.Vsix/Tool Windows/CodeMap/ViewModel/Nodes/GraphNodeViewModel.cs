using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphNodeViewModel : TreeNodeViewModel
	{
		public PXGraphEventSemanticModel GraphSemanticModel { get; }

		public override string Name
		{
			get => GraphSemanticModel.Symbol.Name;
			protected set { }
		}
		protected GraphNodeViewModel(PXGraphEventSemanticModel graphSemanticModel, TreeViewModel tree, bool isExpanded) : 
							 base(tree, isExpanded)
		{
			GraphSemanticModel = graphSemanticModel;
		}

		public static GraphNodeViewModel Create(PXGraphEventSemanticModel graphSemanticModel, TreeViewModel tree, 
												bool isExpanded, bool expandChildren)
		{
			if (graphSemanticModel == null || tree == null)
				return null;

			GraphNodeViewModel graphNodeVM = new GraphNodeViewModel(graphSemanticModel, tree, isExpanded);
			graphNodeVM.AddGraphMemberCategories(expandChildren);
			return graphNodeVM;
		}

		public override void NavigateToItem() => AcuminatorVSPackage.Instance.NavigateToSymbol(GraphSemanticModel.Symbol);
		

		private void AddGraphMemberCategories(bool expandChildren)
		{
			var memberCategories =
				GraphMemberTypeUtils.GetGraphMemberTypes()
									.Select(graphMemberType =>
										 GraphMemberCategoryNodeViewModel.Create(this, graphMemberType, isExpanded: expandChildren));

			Children.AddRange(memberCategories);
		}
	}
}
