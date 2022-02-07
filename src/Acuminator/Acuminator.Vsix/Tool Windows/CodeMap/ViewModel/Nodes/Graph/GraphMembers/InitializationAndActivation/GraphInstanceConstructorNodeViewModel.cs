#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphInstanceConstructorNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.GraphConstructor;

		public InstanceConstructorInfoForCodeMap InstanceConstructorInfo => (InstanceConstructorInfoForCodeMap)MemberInfo;

		public override string Name
		{
			get => VSIXResource.CodeMap_ConstructorNodeName;
			protected set { }
		}

		public GraphInstanceConstructorNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
													 InstanceConstructorInfoForCodeMap instanceConstructorInfo, bool isExpanded = false) :
												base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
													 instanceConstructorInfo, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
