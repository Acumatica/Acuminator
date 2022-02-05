using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedNodeViewModel : GraphMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.CacheAttached;

		public DacGroupingNodeBaseViewModel DacVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public CacheAttachedNodeViewModel(DacGroupingNodeBaseViewModel dacVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
									 base(dacVM?.GraphEventsCategoryVM, dacVM, eventInfo, isExpanded)
		{
			DacVM = dacVM;
			Name = eventInfo.DacFieldName;
		}

		TooltipInfo IElementWithTooltip.CalculateTooltip()
		{
			var attributeStrings = Children.OfType<AttributeNodeViewModel>()
										   .Select(attribute => attribute.CalculateTooltip().Tooltip);
			string aggregatedTooltip = string.Join(Environment.NewLine, attributeStrings);
			return aggregatedTooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(aggregatedTooltip);
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
