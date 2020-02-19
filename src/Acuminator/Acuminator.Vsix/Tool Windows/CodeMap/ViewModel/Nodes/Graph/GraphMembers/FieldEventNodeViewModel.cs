using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacFieldGroupingNodeBaseViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public override Icon NodeIcon => Icon.FieldEvent;

		public FieldEventNodeViewModel(DacFieldGroupingNodeBaseViewModel dacFieldVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = eventInfo.EventType.ToString();
		}

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}