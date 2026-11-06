#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventHandlerNodeViewModel : GraphMemberNodeViewModel
	{
		private readonly string _dacNameWithEventTypeForSearch;

		public DacGroupingNodeBaseViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public override Icon NodeIcon => Icon.RowEvent;

		public RowEventHandlerNodeViewModel(DacGroupingNodeBaseViewModel dacViewModel, GraphRowEventHandlerInfo eventInfo,
											ExpandCreatedNodesFunction isExpandedCalculator) :
									   base(dacViewModel?.GraphEventHandlersCategoryVM!, dacViewModel!, eventInfo, isExpandedCalculator)
		{
			DacViewModel = dacViewModel!;
			Name = eventInfo.EventType.ToString();
			_dacNameWithEventTypeForSearch = $"{DacViewModel.DacName}#{Name}";
		}

		public override bool NameMatchesPattern(string? pattern) => MatchPattern(_dacNameWithEventTypeForSearch, pattern);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
