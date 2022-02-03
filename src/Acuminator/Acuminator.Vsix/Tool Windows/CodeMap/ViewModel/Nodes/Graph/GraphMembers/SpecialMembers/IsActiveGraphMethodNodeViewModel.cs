#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class IsActiveGraphMethodNodeViewModel : GraphMemberNodeViewModel, INodeWithCacheableTooltip
	{
		public override Icon NodeIcon => Icon.IsActiveMethod;

		public IsActiveMethodInfo IsActiveMethodInfo => (IsActiveMethodInfo)MemberInfo;

		public IsActiveGraphMethodNodeViewModel(SpecialGraphMembersCategoryNodeViewModel specialGraphMembersCategoryVM,
												IsActiveMethodInfo isActiveMethodInfo, bool isExpanded = false) :
										   base(specialGraphMembersCategoryVM, specialGraphMembersCategoryVM, isActiveMethodInfo, isExpanded)
		{

		}

		string? INodeWithCacheableTooltip.CalculateTooltip()
		{
			IMethodSymbol isActiveMethod = IsActiveMethodInfo.Symbol;

			if (isActiveMethod.Locations.Length != 1 || isActiveMethod.Locations[0].IsInMetadata || Tree.CodeMapViewModel.Workspace == null)
				return null;

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			var isActiveMethodSyntaxNode = isActiveMethod.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default) as MethodDeclarationSyntax;

			if (isActiveMethodSyntaxNode == null) 
				return null;

			return isActiveMethodSyntaxNode.GetSyntaxNodeStringWithRemovedIndent(tabSize);
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
