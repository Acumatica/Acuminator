using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected override IEnumerable<TreeNodeViewModel> CreateRoots(CodeMapWindowViewModel windowViewModel, CancellationToken cancellation)
		{
			if (windowViewModel.DocumentModel == null)
			{
				return Enumerable.Empty<TreeNodeViewModel>();
			}



			foreach (GraphSemanticModelForCodeMap graph in _documentModel.GraphModels)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var graphNodeVM = GraphNodeViewModel.Create(graph, tree, isExpanded: true, expandChildren: false);

				if (graphNodeVM != null)
				{
					tree.RootItems.Add(graphNodeVM);
				}
			}
		}

		public override IEnumerable<TreeNodeViewModel> CreateChildrenNodes(TreeNodeViewModel parentNode, CancellationToken cancellation)
		{
			parentNode.ThrowOnNull(nameof(parentNode));

			switch (parentNode)
			{
				default:
					return Enumerable.Empty<TreeNodeViewModel>();
			}
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraph(GraphNodeViewModel graphNode)
		{

		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategories(GraphNodeViewModel graphNode)
		{

		}

		
	}
}
