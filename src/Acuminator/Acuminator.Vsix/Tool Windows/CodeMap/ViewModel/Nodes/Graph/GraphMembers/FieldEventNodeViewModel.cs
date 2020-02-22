using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;

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

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; } =
			new ExtendedObservableCollection<ExtraInfoViewModel>(
				new IconViewModel(Icon.FieldEvent));

		public FieldEventNodeViewModel(DacFieldGroupingNodeBaseViewModel dacFieldVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
								  base(dacFieldVM?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = eventInfo.EventType.ToString();
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}