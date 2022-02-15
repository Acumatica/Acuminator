#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphStaticConstructorNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.StaticGraphConstructor;

		public StaticConstructorInfo StaticConstructorInfo => (StaticConstructorInfo)MemberInfo;

		public override string Name
		{
			get => VSIXResource.CodeMap_StaticConstructorNodeName;
			protected set { }
		}

		public GraphStaticConstructorNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												   StaticConstructorInfo staticConstructorInfo, bool isExpanded = false) :
											  base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
												   staticConstructorInfo, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
