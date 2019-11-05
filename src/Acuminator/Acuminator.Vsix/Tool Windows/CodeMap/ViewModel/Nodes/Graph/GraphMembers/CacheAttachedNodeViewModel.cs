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

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; } =
			new ExtendedObservableCollection<ExtraInfoViewModel>(
				new IconViewModel(Icon.CacheAttached));

		public CacheAttachedNodeViewModel(DacGroupingNodeBaseViewModel dacVM, GraphFieldEventInfo eventInfo, bool isExpanded = false) :
									 base(dacVM?.GraphEventsCategoryVM, eventInfo, isExpanded)
		{
			DacVM = dacVM;
			Name = eventInfo.DacFieldName;			
		}

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																		 CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
