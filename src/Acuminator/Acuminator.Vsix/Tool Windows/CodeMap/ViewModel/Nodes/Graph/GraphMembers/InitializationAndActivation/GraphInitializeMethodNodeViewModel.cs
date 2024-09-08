#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphInitializeMethodNodeViewModel : GraphMemberNodeViewModel
	{
		protected readonly string _name;

		public override string Name
		{
			get => _name;
			protected set { }
		}

		public override Icon NodeIcon => Icon.InitializeMethodGraph;

		public InitializeMethodInfo InitializeMethod => (InitializeMethodInfo)MemberInfo;

		public GraphInitializeMethodNodeViewModel(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
												  InitializeMethodInfo initializeMethodInfo, bool isExpanded = false) :
											 base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
												  initializeMethodInfo, isExpanded)
		{
			_name = GetName(initializeMethodInfo.Symbol);
		}

		private string GetName(IMethodSymbol methodSymbol)
		{
			if (methodSymbol.MethodKind != MethodKind.ExplicitInterfaceImplementation)
				return methodSymbol.Name;

			int lastDotIndex = methodSymbol.Name.LastIndexOf('.');

			if (lastDotIndex < 0)
				return methodSymbol.Name;

			return methodSymbol.Name[(lastDotIndex + 1)..];
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => 
			treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
