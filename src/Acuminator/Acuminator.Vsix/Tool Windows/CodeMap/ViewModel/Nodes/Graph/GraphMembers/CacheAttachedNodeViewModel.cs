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
		public DacFieldGroupingNodeBaseViewModel DacFieldVM { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public CacheAttachedNodeViewModel(DacFieldGroupingNodeBaseViewModel dacFieldVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
									 base(dacFieldVM?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacFieldVM = dacFieldVM;
			Name = eventInfo.DacFieldName;			
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																		 CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
