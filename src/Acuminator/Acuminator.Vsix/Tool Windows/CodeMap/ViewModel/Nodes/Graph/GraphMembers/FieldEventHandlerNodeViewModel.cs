#nullable enable

using System;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventHandlerNodeViewModel : GraphMemberNodeViewModel
	{
		private readonly string _dacNameWithFieldNameWithEventTypeForSearch;

		public override Icon NodeIcon => Icon.FieldEvent;

		public DacFieldGroupingNodeBaseViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventHandlerNodeViewModel(DacFieldGroupingNodeBaseViewModel dacFieldVM, GraphFieldEventHandlerInfo eventHandlerInfo,
											  ExpandCreatedNodesFunction isExpandedCalculator) :
										 base(dacFieldVM?.GraphEventHandlersCategoryVM!, dacFieldVM!, eventHandlerInfo, isExpandedCalculator)
		{
			DacFieldVM = dacFieldVM!;
			Name = eventHandlerInfo.EventType.ToString();
			_dacNameWithFieldNameWithEventTypeForSearch = $"{DacFieldVM.DacVM.Name}#{DacFieldVM.DacFieldName}#{Name}";
		}

		public override bool NameMatchesPattern(string? pattern) => MatchPattern(_dacNameWithFieldNameWithEventTypeForSearch, pattern);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}