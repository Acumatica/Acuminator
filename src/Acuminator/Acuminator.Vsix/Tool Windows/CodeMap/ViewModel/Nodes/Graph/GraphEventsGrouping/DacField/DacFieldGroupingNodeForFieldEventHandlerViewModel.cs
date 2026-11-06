#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeForFieldEventHandlerViewModel : DacFieldGroupingNodeBaseViewModel
	{
		public DacFieldGroupingNodeForFieldEventHandlerViewModel(DacGroupingNodeBaseViewModel dacVM, string dacFieldName, 
																 IEnumerable<GraphFieldEventHandlerInfo> dacFieldEventHandlers, 
																 ExpandCreatedNodesFunction isExpandedCalculator) :
															base(dacVM, dacFieldName, dacFieldEventHandlers, isExpandedCalculator)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}