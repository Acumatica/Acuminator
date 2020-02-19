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
	public class CacheAttachedCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public CacheAttachedCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
											 base(graphViewModel, GraphMemberType.CacheAttached, isExpanded)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.CacheAttachedEvents;

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}
