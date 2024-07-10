﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;


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

		protected override TreeNodeViewModel? CreateRoot(ISemanticModel rootSemanticModel, TreeViewModel tree)
		{
			return rootSemanticModel switch
			{
				GraphSemanticModelForCodeMap graphSemanticModel => CreateGraphNode(graphSemanticModel, tree),
				DacSemanticModel dacSemanticModel               => CreateDacNode(dacSemanticModel, tree),
				_                                               => null,
			};
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(TreeNodeViewModel node)
		{
			var generatedChildren = base.VisitNode(node);

			if (generatedChildren == null || ReferenceEquals(generatedChildren, DefaultValue))
				return generatedChildren;

			return TreeInitialSorter.SortGeneratedChildren(node, generatedChildren);
		}
	}
}