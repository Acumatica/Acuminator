#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacBqlFieldNodeViewModel : DacMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.DacBqlField;

		public DacBqlFieldInfo BqlFieldInfo => (MemberInfo as DacBqlFieldInfo)!;

		public DacBqlFieldNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent,
										DacBqlFieldInfo bqlFieldInfo, bool isExpanded = false) :
										base(dacMemberCategoryVM, parent, bqlFieldInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			if (Tree.CodeMapViewModel.Workspace == null)
			{
				string tooltipText = BqlFieldInfo.Node.ToString();
				return new TooltipInfo(tooltipText);
			}
			else
			{
				int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
				string tooltipText = BqlFieldInfo.Node.GetSyntaxNodeStringWithRemovedIndent(tabSize);
				return new TooltipInfo(tooltipText);
			}
		}
	}
}
