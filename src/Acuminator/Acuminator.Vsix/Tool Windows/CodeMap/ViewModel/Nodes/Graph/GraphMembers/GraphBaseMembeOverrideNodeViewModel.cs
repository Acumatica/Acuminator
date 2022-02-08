#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphBaseMembeOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public bool IsPersistMethodOverride => BaseMemberOverrideInfo.IsPersistMethodOverride;

		public override Icon NodeIcon { get; }

		public BaseMemberOverrideInfo BaseMemberOverrideInfo => (BaseMemberOverrideInfo)MemberInfo;

		public override string Name
		{
			get;
			protected set;
		}

		public GraphBaseMembeOverrideNodeViewModel(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategoryVM, 
												   BaseMemberOverrideInfo baseMemberOverrideInfo, bool isExpanded = false) :
											  base(graphBaseMemberOverridesCategoryVM, graphBaseMemberOverridesCategoryVM, baseMemberOverrideInfo, isExpanded)
		{
			NodeIcon = GetNodeIcon();

			if (MemberSymbol is IMethodSymbol method && method.Name == EventsNames.CommonEventHandlerWithGenericSignatureName)
			{	
				Name = GetNameForOverridenGraphEventHandler(method) ?? baseMemberOverrideInfo.Symbol.Name;
			}
			else
			{
				Name = baseMemberOverrideInfo.Symbol.Name;
			}
		}

		private Icon GetNodeIcon() => MemberSymbol switch
		{
			IMethodSymbol method => IsPersistMethodOverride 
										? Icon.PersistMethodOverride 
										: Icon.MethodOverrideGraph,
			IPropertySymbol _    => Icon.PropertyOverrideGraph,
			IEventSymbol _       => Icon.EventOverrideGraph,
			_                    => Icon.MethodOverrideGraph
		};

		private string? GetNameForOverridenGraphEventHandler(IMethodSymbol graphEventHandler)
		{
			var methodNode = graphEventHandler.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default) as MethodDeclarationSyntax;

			if (methodNode == null)
				return null;

			var parameterTypesString =  methodNode.ParameterList.Parameters
																.Select(p => p.Type.ToString())
																.Join(", ");
			string name = methodNode.Identifier.ToString() + $"({parameterTypesString})";
			return name;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
