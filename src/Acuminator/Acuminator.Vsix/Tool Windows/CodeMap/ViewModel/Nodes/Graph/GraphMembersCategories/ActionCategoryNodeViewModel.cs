using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ActionCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public ActionCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									  base(graphViewModel, GraphMemberType.Action, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.Actions;

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}
