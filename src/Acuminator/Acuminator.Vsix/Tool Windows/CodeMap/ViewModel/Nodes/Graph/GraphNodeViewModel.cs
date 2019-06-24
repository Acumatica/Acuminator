using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphNodeViewModel : TreeNodeViewModel
	{
		public GraphSemanticModelForCodeMap CodeMapGraphModel { get; }

		public PXGraphEventSemanticModel GraphSemanticModel => CodeMapGraphModel.GraphModel; 

		public override string Name
		{
			get => GraphSemanticModel.Symbol.Name;
			protected set { }
		}
		protected GraphNodeViewModel(GraphSemanticModelForCodeMap codeMapGraphModel, TreeViewModel tree, bool isExpanded) : 
							 base(tree, isExpanded)
		{
			CodeMapGraphModel = codeMapGraphModel;
		}

		public static GraphNodeViewModel Create(GraphSemanticModelForCodeMap codeMapGraphModel, TreeViewModel tree, 
												bool isExpanded, bool expandChildren)
		{
			if (codeMapGraphModel == null || tree == null)
				return null;

			GraphNodeViewModel graphNodeVM = new GraphNodeViewModel(codeMapGraphModel, tree, isExpanded);
			graphNodeVM.AddGraphMemberCategories(expandChildren);
			return graphNodeVM;
		}

		public override Task NavigateToItemAsync() => GraphSemanticModel.Symbol.NavigateToAsync();
		

		private void AddGraphMemberCategories(bool expandChildren)
		{
			var memberCategories =
				GraphMemberTypeUtils.GetGraphMemberTypes()
									.Select(graphMemberType =>
										 GraphMemberCategoryNodeViewModel.Create(this, graphMemberType, isExpanded: expandChildren))
									.Where(graphMemberCategory => graphMemberCategory != null);

			Children.AddRange(memberCategories);
		}
	}
}
