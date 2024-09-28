#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected CodeMapTreeInitialSorter TreeInitialSorter { get; }

		public DefaultCodeMapTreeBuilder(CodeMapTreeInitialSorter? customSorter = null)
		{
			TreeInitialSorter = customSorter ?? 
								new CodeMapTreeInitialSorter(defaultSortType: SortType.Declaration, defaultSortDirection: SortDirection.Ascending);
		}

		protected override TreeNodeViewModel? CreateRoot(ISemanticModel rootSemanticModel, TreeNodeViewModel? rootParent, TreeViewModel tree)
		{
			return rootSemanticModel switch
			{
				GraphSemanticModelForCodeMap graphSemanticModel => CreateGraphNode(graphSemanticModel, tree),
				DacSemanticModelForCodeMap dacSemanticModel 	=> CreateDacNode(dacSemanticModel, rootParent, tree),
				_ 												=> null,
			};
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(TreeNodeViewModel node)
		{
			var generatedChildren = base.VisitNode(node)?.ToList();

			if (generatedChildren == null || ReferenceEquals(generatedChildren, DefaultValue))
				return generatedChildren;

			return TreeInitialSorter.SortGeneratedChildren(node, generatedChildren);
		}
	}
}