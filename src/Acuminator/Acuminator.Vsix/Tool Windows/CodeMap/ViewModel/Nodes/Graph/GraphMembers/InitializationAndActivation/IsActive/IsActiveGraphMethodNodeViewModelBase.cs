#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class IsActiveGraphMethodNodeViewModelBase : GraphMemberNodeViewModel, IElementWithTooltip
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public override Icon NodeIcon => Icon.IsActiveMethodGraph;

		protected IsActiveGraphMethodNodeViewModelBase(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategoryVM,
													   NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol> isActiveMethodMemberInfo, 
													   bool isExpanded = false) :
												  base(graphInitializationAndActivationCategoryVM, graphInitializationAndActivationCategoryVM,
													   isActiveMethodMemberInfo, isExpanded)
		{
		}

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			if (MemberInfo is not NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol> isActiveMethodInfo)
				return null;

			IMethodSymbol isActiveMethod = isActiveMethodInfo.Symbol;

			if (isActiveMethod.Locations.Length != 1 || isActiveMethod.Locations[0].IsInMetadata || Tree.CodeMapViewModel.Workspace == null)
				return null;

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			var isActiveMethodSyntaxNode = isActiveMethod.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default) as MethodDeclarationSyntax;

			if (isActiveMethodSyntaxNode == null) 
				return null;

			string tooltip = isActiveMethodSyntaxNode.GetSyntaxNodeStringWithRemovedIndent(tabSize);
			return new TooltipInfo(tooltip) { TrimExcess = true };
		}
	}
}
