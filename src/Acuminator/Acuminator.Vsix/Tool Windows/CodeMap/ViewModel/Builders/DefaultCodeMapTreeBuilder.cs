using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected override TreeNodeViewModel CreateRoot(ISemanticModel rootSemanticModel, TreeViewModel tree, bool expandRoots,
														CancellationToken cancellation)
		{
			switch (rootSemanticModel)
			{
				case GraphSemanticModelForCodeMap graphSemanticModel:
					return CreateGraphNode(graphSemanticModel, tree, expandRoots);

				case DacSemanticModel dacSemanticModel:
					return CreateDacNode(dacSemanticModel, tree, expandRoots);

				default:
					return null;
			}
		}
	}
}