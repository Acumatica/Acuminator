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
	public class CacheAttachedCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public CacheAttachedCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
									base(graphViewModel, GraphMemberType.CacheAttached, isExpanded)
		{
		}

		protected override GraphEventNodeByDacConstructor EventNodeByDacConstructor { get; } =
			(dacGroupVM, eventInfo, isExpanded) => new CacheAttachedNodeViewModel(dacGroupVM, eventInfo, isExpanded);

		protected override IEnumerable<GraphNodeSymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.CacheAttachedEvents;
	}
}
