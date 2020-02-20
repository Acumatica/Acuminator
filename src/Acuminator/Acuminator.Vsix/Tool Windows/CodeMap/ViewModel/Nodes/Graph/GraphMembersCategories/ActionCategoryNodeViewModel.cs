using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
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

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
