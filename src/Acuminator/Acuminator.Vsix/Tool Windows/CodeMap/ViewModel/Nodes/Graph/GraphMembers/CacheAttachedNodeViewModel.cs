using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedNodeViewModel : GraphMemberNodeViewModel
	{
		public override Icon NodeIcon => Icon.CacheAttached;

		public DacGroupingNodeBaseViewModel DacVM { get; }

		public override string Tooltip
		{
			get
			{
				var attributeStrings = Children.OfType<AttributeNodeViewModel>().Select(attribute => attribute.Tooltip);
				return string.Join(Environment.NewLine, attributeStrings);
			}
		}

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

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
