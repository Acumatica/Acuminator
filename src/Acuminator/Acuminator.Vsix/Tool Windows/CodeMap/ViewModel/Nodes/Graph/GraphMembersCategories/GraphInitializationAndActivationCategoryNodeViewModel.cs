#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphInitializationAndActivationCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public override Icon NodeIcon => Icon.InitializationAndActivationGraphCategory;

		public GraphInitializationAndActivationCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
																base(graphViewModel, GraphMemberType.InitializationAndActivation, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols()
		{
			if (GraphSemanticModel.IsActiveMethodInfo != null)
				yield return GraphSemanticModel.IsActiveMethodInfo;

			foreach (StaticConstructorInfo staticConstructor in GraphSemanticModel.StaticConstructors)
			{
				if (!staticConstructor.Symbol.IsImplicitlyDeclared)
					yield return staticConstructor;
			}

			foreach (InstanceConstructorInfoForCodeMap constructor in CodeMapGraphModel.InstanceConstructors)
			{
				yield return constructor;
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
