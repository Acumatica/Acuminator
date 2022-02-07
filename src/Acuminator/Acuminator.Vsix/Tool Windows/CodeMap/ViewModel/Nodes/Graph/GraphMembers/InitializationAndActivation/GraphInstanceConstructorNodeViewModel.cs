#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphInstanceConstructorNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.GraphConstructor;

		public InstanceConstructorInfoForCodeMap InstanceConstructorInfo => (InstanceConstructorInfoForCodeMap)MemberInfo;

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
