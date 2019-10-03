using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		private string _name;

		public override string Name
		{
			get => _name;
			protected set
			{
				if (_name != value)
				{
					_name = value;
					NotifyPropertyChanged();
				}
			}
		}

		public override Icon NodeIcon => Icon.GraphEventCategory;

		protected override bool AllowNavigation => false;

		protected GraphEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType, bool isExpanded) :
										     base(graphViewModel, graphMemberType, isExpanded)
		{
			_name = CategoryDescription;
		}

		public override void AcceptBuilder(TreeBuilderBase treeBuilder, bool expandRoots, CancellationToken cancellation)
		{
			base.AcceptBuilder(treeBuilder, expandRoots, cancellation);
			int eventsCount = Children.OfType<DacGroupingNodeBaseViewModel>()
									  .Sum(dacVM => dacVM.EventsCount);
			if (Children.Count <= 0)
				return;

			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
