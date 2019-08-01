using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities.Navigation;
using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphNodeViewModel : TreeNodeViewModel
	{
		private int _currentNavigationIndex;

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

		private void AddGraphMemberCategories(bool expandChildren)
		{
			var memberCategories =
				GraphMemberTypeUtils.GetGraphMemberTypes()
									.Select(graphMemberType =>
										 GraphMemberCategoryNodeViewModel.Create(this, graphMemberType, isExpanded: expandChildren))
									.Where(graphMemberCategory => graphMemberCategory != null);

			Children.AddRange(memberCategories);
		}

		public override Task NavigateToItemAsync()
		{
			var syntaxReferences = GraphSemanticModel.Symbol.DeclaringSyntaxReferences;

			switch (syntaxReferences.Length)
			{
				case 0:
					return Task.CompletedTask;
				case 1:
					return GraphSemanticModel.Symbol.NavigateToAsync();
				default:

					if (_currentNavigationIndex >= syntaxReferences.Length)
						return Task.CompletedTask;

					SyntaxReference reference = syntaxReferences[_currentNavigationIndex];
					_currentNavigationIndex = (_currentNavigationIndex + 1) % syntaxReferences.Length;

					return GraphSemanticModel.Symbol.NavigateToAsync(reference);
			}		
		}	
	}
}