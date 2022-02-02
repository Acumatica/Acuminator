#nullable enable

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
	public class SpecialGraphMembersCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public override Icon NodeIcon => Icon.SpecialMemberCategory;

		public SpecialGraphMembersCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
												   base(graphViewModel, GraphMemberType.SpecialMember, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.IsActiveMethod?.ToEnumerable() ?? Enumerable.Empty<SymbolItem>();

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
