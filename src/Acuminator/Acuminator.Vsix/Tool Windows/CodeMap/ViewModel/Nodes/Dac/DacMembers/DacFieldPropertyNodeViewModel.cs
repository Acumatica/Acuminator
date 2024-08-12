#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldPropertyNodeViewModel : DacMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.DacFieldProperty;

		public DacPropertyInfo PropertyInfo => (MemberInfo as DacPropertyInfo)!;

		public DacFieldPropertyNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
											 DacPropertyInfo propertyInfo, bool isExpanded = false) :
										base(dacMemberCategoryVM, parent, propertyInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var attributeStrings = Children.OfType<AttributeNodeViewModel>()
										   .Select(attribute => attribute.CalculateTooltip().Tooltip);
			string aggregatedTooltip = string.Join(Environment.NewLine, attributeStrings);
			return aggregatedTooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(aggregatedTooltip);
		}
	}
}
