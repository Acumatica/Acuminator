using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ViewNodeViewModel : GraphMemberNodeViewModel
	{
		public DataViewInfo ViewInfo => MemberInfo as DataViewInfo;

		public string Tooltip
		{
			get;
			protected set;
		}

		public ViewNodeViewModel(ViewCategoryNodeViewModel viewCategoryVM, DataViewInfo viewInfo, bool isExpanded = false) :
							base(viewCategoryVM, viewInfo, isExpanded)
		{
			AddViewDelegate();
			Tooltip = GetTooltip(viewInfo);
		}	

		protected virtual void AddViewDelegate()
		{
			if (MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(MemberSymbol.Name, out DataViewDelegateInfo viewDelegate))
			{
				Children.Add(new GraphMemberInfoNodeViewModel(this, viewDelegate, GraphMemberInfoType.ViewDelegate));
			}
		}

		protected string GetTooltip(DataViewInfo viewInfo)
		{
			if (viewInfo.Symbol?.Locations.Length != 1 || viewInfo.Symbol.Locations[0].IsInMetadata || Tree.CodeMapViewModel.Workspace == null)
			{
				return viewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			SyntaxNode viewSyntaxNode = viewInfo.Symbol.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default);

			switch (viewSyntaxNode)
			{
				case PropertyDeclarationSyntax propertyDeclaration:
					return propertyDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize);

				case VariableDeclarationSyntax variableDeclaration:
					return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize);

				case VariableDeclaratorSyntax variableDeclarator 
				when variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration:
					return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize);

				default:
					return viewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}
		}
	}
}
