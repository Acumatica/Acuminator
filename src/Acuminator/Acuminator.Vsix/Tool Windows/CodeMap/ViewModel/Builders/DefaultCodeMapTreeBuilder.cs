using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected CodeMapTreeInitialSorter TreeInitialSorter { get; }

		public DefaultCodeMapTreeBuilder(CodeMapTreeInitialSorter customSorter)
		{
			TreeInitialSorter = customSorter ?? 
								new CodeMapTreeInitialSorter(defaultSortType: SortType.Declaration, defaultSortDirection: SortDirection.Ascending);
		}

		protected override TreeNodeViewModel CreateRoot(ISemanticModel rootSemanticModel, TreeViewModel tree)
		{
			switch (rootSemanticModel)
			{
				case GraphSemanticModelForCodeMap graphSemanticModel:
					return CreateGraphNode(graphSemanticModel, tree);

				case DacSemanticModel dacSemanticModel:
					return CreateDacNode(dacSemanticModel, tree);

				default:
					return null;
			}
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(TreeNodeViewModel node)
		{
			var generatedChildren = base.VisitNode(node);

			if (generatedChildren == null || ReferenceEquals(generatedChildren, DefaultValue))
				return generatedChildren;

			return TreeInitialSorter.SortGeneratedChildren(node, generatedChildren);
		}
	}
}