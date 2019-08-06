using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PXOverridesCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public PXOverridesCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
										   base(graphViewModel, GraphMemberType.PXOverride, isExpanded)
		{
			
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => CodeMapGraphModel.PXOverrides;

		protected override void AddCategoryMembers()
		{
			IEnumerable<SymbolItem> categoryTreeNodes = GetCategoryGraphNodeSymbols();

			if (categoryTreeNodes.IsNullOrEmpty())
				return;

			var graphMemberViewModels = from pxOverrideInfo in categoryTreeNodes.OfType<PXOverrideInfoForCodeMap>()
										orderby pxOverrideInfo.SymbolBase.Name
										select new PXOverrideNodeViewModel(this, pxOverrideInfo);

			Children.AddRange(graphMemberViewModels);
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
