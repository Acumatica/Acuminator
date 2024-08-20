﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.FieldEvent;

		public DacFieldGroupingNodeBaseViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventNodeViewModel(DacFieldGroupingNodeBaseViewModel dacFieldVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, dacFieldVM, eventInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM!;
			Name = eventInfo.EventType.ToString();
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}