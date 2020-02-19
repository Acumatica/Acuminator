using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeForFieldEventViewModel : DacFieldGroupingNodeBaseViewModel
	{
		public DacFieldGroupingNodeForFieldEventViewModel(DacGroupingNodeBaseViewModel dacVM, string dacFieldName, 
														  IEnumerable<GraphFieldEventInfo> dacFieldEvents, bool isExpanded) :
													 base(dacVM, dacFieldName, dacFieldEvents, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);
	}
}