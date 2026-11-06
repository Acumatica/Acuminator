#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphConfigureMethodNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.ConfigureMethodGraph;

		public ConfigureMethodInfo ConfigureMethod => (ConfigureMethodInfo)MemberInfo;

		public GraphConfigureMethodNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												 ConfigureMethodInfo configureMethodInfo, ExpandCreatedNodesFunction isExpandedCalculator) :
											base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
												 configureMethodInfo, isExpandedCalculator)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => 
			treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
