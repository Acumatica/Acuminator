#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class NonBqlDacPropertyNodeViewModel : DacMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.DacNonBqlProperty;

		public override bool IconDependsOnCurrentTheme => true;

		public DacPropertyInfo PropertyInfo => (MemberInfo as DacPropertyInfo)!;

		public NonBqlDacPropertyNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
											  DacPropertyInfo nonBqlPropertyInfo, ExpandCreatedNodesFunction isExpandedCalculator) :
										 base(dacMemberCategoryVM, parent, nonBqlPropertyInfo, isExpandedCalculator)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			string? tooltip = GetTooltip();
			return tooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(tooltip) { TrimExcess = true };
		}

		private string? GetTooltip()
		{
			if (Tree.CodeMapViewModel.Workspace == null)
				return PropertyInfo.Node?.ToString();

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			return PropertyInfo.Node.GetSyntaxNodeStringWithRemovedIndent(tabSize);
		}
	}
}
