﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventCategoryNodeViewModel : GraphEventCategoryNodeViewModel
	{
		public FieldEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
										  base(graphViewModel, GraphMemberCategory.FieldEvent, isExpanded)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.FieldDefaultingEvents
							  .Concat(GraphSemanticModel.FieldVerifyingEvents)
							  .Concat(GraphSemanticModel.FieldSelectingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatingEvents)
							  .Concat(GraphSemanticModel.FieldUpdatedEvents)
							  .Concat(GraphSemanticModel.ExceptionHandlingEvents)
							  .Concat(GraphSemanticModel.CommandPreparingEvents);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
