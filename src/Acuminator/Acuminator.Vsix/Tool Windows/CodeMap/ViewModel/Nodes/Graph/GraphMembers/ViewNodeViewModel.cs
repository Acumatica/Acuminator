using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ViewNodeViewModel : GraphMemberNodeViewModel
	{
		public DataViewInfo ViewInfo => MemberInfo as DataViewInfo;

		public override string Tooltip => GetTooltip();

		public override Icon NodeIcon => Icon.View;

		public ViewNodeViewModel(ViewCategoryNodeViewModel viewCategoryVM, DataViewInfo viewInfo, bool isExpanded = false) :
							base(viewCategoryVM, viewInfo, isExpanded)
		{
		}	

		private string GetTooltip()
		{
			if (ViewInfo.Symbol?.Locations.Length != 1 || ViewInfo.Symbol.Locations[0].IsInMetadata || Tree.CodeMapViewModel.Workspace == null)
			{
				return ViewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			SyntaxNode viewSyntaxNode = ViewInfo.Symbol.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default);

			switch (viewSyntaxNode)
			{
				case PropertyDeclarationSyntax propertyDeclaration:
					{
						int prependLength = GetPrependLength(propertyDeclaration.Modifiers);
						return propertyDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				case VariableDeclarationSyntax variableDeclaration:
					{
						int prependLength = GetPrependLength((variableDeclaration.Parent as FieldDeclarationSyntax)?.Modifiers);
						return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				case VariableDeclaratorSyntax variableDeclarator 
				when variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration:
					{
						int prependLength = GetPrependLength((variableDeclaration.Parent as FieldDeclarationSyntax)?.Modifiers);
						return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				default:
					return ViewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}
		}

		private static int GetPrependLength(SyntaxTokenList? modifiers) => modifiers != null
			? modifiers.Value.FullSpan.End - modifiers.Value.Span.Start
			: 0;

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}
