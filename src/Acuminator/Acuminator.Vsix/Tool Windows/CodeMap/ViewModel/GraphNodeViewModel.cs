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

		private GraphNodeViewModel(PXGraphSemanticModel graphSemanticModel, TreeViewModel tree, 
								   string name, bool isExpanded) : 
							 base(tree, name, isExpanded)
		{
			
		}

		public static GraphNodeViewModel Create(PXGraphSemanticModel graphSemanticModel, TreeViewModel tree)
		{
			if (graphSemanticModel == null || tree == null)
				return null;

			GraphNodeViewModel graphNodeVM = new GraphNodeViewModel(graphSemanticModel, tree,
																	graphSemanticModel.Symbol.Name, isExpanded: false);


			return graphNodeVM;
		}
	}
}
