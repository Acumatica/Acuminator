﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacGroupingNodeForFieldEventViewModel : DacGroupingNodeBaseViewModel
	{
		public ImmutableArray<GraphFieldEventInfo> AllFieldEvents { get; }

		public DacGroupingNodeForFieldEventViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName,
														IEnumerable<GraphFieldEventInfo> fieldEvents, bool isExpanded) :
												   base(graphEventsCategoryVM, dacName, isExpanded)
		{
			AllFieldEvents = fieldEvents?.ToImmutableArray() ?? ImmutableArray.Create<GraphFieldEventInfo>();
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}