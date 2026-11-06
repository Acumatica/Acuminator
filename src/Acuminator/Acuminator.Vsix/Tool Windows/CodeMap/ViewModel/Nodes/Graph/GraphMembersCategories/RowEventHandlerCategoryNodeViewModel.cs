#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventHandlerCategoryNodeViewModel : GraphEventHandlerCategoryNodeViewModel
	{
		public RowEventHandlerCategoryNodeViewModel(GraphNodeViewModel graphViewModel, TreeNodeViewModel parent,
													ExpandCreatedNodesFunction isExpandedCalculator) : 
											   base(graphViewModel, parent, GraphMemberCategory.RowEvent, isExpandedCalculator)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.DeclaredEventHandlers.RowInsertingEventHandlers
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowInsertedEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowSelectingEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowSelectedEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowUpdatingEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowUpdatedEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowDeletingEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowDeletedEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowPersistingEventHandlers)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowPersistedEventHandlers);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
