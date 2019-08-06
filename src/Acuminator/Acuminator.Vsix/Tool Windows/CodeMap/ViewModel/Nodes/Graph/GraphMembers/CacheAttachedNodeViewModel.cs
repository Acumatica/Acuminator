using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedNodeViewModel : GraphMemberNodeViewModel
	{
		public DacGroupingNodeBaseViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public CacheAttachedNodeViewModel(DacGroupingNodeBaseViewModel dacViewModel, GraphFieldEventInfo eventInfo,
										  bool isExpanded = false) :
									 base(dacViewModel?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacViewModel = dacViewModel;
			Name = eventInfo.DacFieldName;			
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																		 CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
