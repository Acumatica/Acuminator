using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacGroupingNodeBaseViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; } =
			new ExtendedObservableCollection<ExtraInfoViewModel>(
				new IconViewModel(Icon.RowEvent));

		public RowEventNodeViewModel(DacGroupingNodeBaseViewModel dacViewModel, GraphRowEventInfo eventInfo, bool isExpanded = false) :
								base(dacViewModel?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacViewModel = dacViewModel;
			Name = eventInfo.EventType.ToString();
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																		 CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
