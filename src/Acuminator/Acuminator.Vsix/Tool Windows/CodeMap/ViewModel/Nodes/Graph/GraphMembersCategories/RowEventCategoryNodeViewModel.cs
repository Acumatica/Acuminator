#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public RowEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
										base(graphViewModel, GraphMemberType.RowEvent, isExpanded)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.RowInsertingEvents
							  .Concat(GraphSemanticModel.RowInsertedEvents)
							  .Concat(GraphSemanticModel.RowSelectingEvents)
							  .Concat(GraphSemanticModel.RowSelectedEvents)
							  .Concat(GraphSemanticModel.RowUpdatingEvents)
							  .Concat(GraphSemanticModel.RowUpdatedEvents)
							  .Concat(GraphSemanticModel.RowDeletingEvents)
							  .Concat(GraphSemanticModel.RowDeletedEvents)
							  .Concat(GraphSemanticModel.RowPersistingEvents)
							  .Concat(GraphSemanticModel.RowPersistedEvents);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
