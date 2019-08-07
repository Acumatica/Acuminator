using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected override IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, bool expandRoots, CancellationToken cancellation)
		{
			if (tree.CodeMapViewModel.DocumentModel == null)
				yield break;

			foreach (GraphSemanticModelForCodeMap graph in tree.CodeMapViewModel.DocumentModel.GraphModels)
			{
				cancellation.ThrowIfCancellationRequested();
				yield return CreateGraphNode(graph, tree, expandRoots);
			}
		}
	}
}