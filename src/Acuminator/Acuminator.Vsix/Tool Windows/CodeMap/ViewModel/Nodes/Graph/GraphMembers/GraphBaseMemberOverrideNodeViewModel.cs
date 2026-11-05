#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphBaseMemberOverrideNodeViewModel : GraphMemberNodeViewModel
	{
		public MemberOverrideKind OverrideKind => BaseMemberOverrideInfo.OverrideKind;

		public override Icon NodeIcon { get; }

		public override bool IconDependsOnCurrentTheme => 
			OverrideKind is MemberOverrideKind.NormalPropertyOverride or MemberOverrideKind.NormalEventOverride;

		public BaseMemberOverrideInfo BaseMemberOverrideInfo => (BaseMemberOverrideInfo)MemberInfo;

		public override string Name
		{
			get;
			protected set;
		}

		public GraphBaseMemberOverrideNodeViewModel(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategoryVM, 
												    BaseMemberOverrideInfo baseMemberOverrideInfo, ExpandCreatedNodesFunction isExpandedCalculator) :
											   base(graphBaseMemberOverridesCategoryVM, graphBaseMemberOverridesCategoryVM, baseMemberOverrideInfo, 
												    isExpandedCalculator)
		{
			NodeIcon = GetNodeIcon();

			if (MemberSymbol is IMethodSymbol method && method.Name == Events.Names.CommonEventHandlerWithGenericSignatureName)
			{	
				Name = GetNameForOverriddenGraphEventHandler(method) ?? baseMemberOverrideInfo.Symbol.Name;
			}
			else
			{
				Name = baseMemberOverrideInfo.Symbol.Name;
			}
		}

		private Icon GetNodeIcon() => OverrideKind switch
		{
			MemberOverrideKind.NormalMethodOverride		=> Icon.MethodOverrideGraph,
			MemberOverrideKind.NormalPropertyOverride  	=> Icon.PropertyOverrideGraph,
			MemberOverrideKind.NormalEventOverride		=> Icon.EventOverrideGraph,

			MemberOverrideKind.PersistMethodOverride	=> Icon.PersistMethodOverride,
			MemberOverrideKind.ConfigureMethodOverride 	=> Icon.ConfigureMethodGraph,
			MemberOverrideKind.InitializeMethodOverride => Icon.InitializeMethodGraph,
			_ 											=> Icon.None
		};

		private string? GetNameForOverriddenGraphEventHandler(IMethodSymbol graphEventHandler)
		{
			var methodNode = graphEventHandler.GetSyntax(AcuminatorVSPackage.Instance?.DisposalToken ?? default) as MethodDeclarationSyntax;

			if (methodNode == null)
				return null;

			var parameterTypesString =  methodNode.ParameterList.Parameters
																.Where(p => p.Type != null)
																.Select(p => p.Type!.ToString())
																.Join(", ");
			string name = methodNode.Identifier.ToString() + $"({parameterTypesString})";
			return name;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
