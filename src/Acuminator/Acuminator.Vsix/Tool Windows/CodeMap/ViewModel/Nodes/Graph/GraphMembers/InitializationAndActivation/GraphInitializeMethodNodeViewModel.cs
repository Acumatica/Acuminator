#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphInitializeMethodNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.InitializeMethodGraph;

		public InitializeMethodInfo InitializeMethod => (InitializeMethodInfo)MemberInfo;

		public GraphInitializeMethodNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												  InitializeMethodInfo initializeMethodInfo, bool isExpanded = false) :
											 base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
												  initializeMethodInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => 
			treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
