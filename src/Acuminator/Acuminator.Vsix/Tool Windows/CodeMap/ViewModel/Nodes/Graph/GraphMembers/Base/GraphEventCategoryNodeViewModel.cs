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

		protected override bool AllowNavigation => false;

		protected GraphEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, GraphMemberType graphMemberType, bool isExpanded) :
										     base(graphViewModel, graphMemberType, isExpanded)
		{
			_name = CategoryDescription;
		}

		public virtual GraphMemberNodeViewModel CreateNewEventVM<TEventNodeParent>(TEventNodeParent eventNodeParent, GraphEventInfoBase eventInfo,
																				   bool isExpanded)
		where TEventNodeParent : TreeNodeViewModel
		{
			return new GraphMemberNodeViewModel(this, eventInfo, isExpanded);
		}

		public abstract IEnumerable<TreeNodeViewModel> GetEventsViewModelsForDAC(DacEventsGroupingNodeViewModel dacVM, 
																				 IEnumerable<GraphEventInfoBase> graphEventsForDAC,
																				 bool areChildrenExpanded);

		public override void AcceptBuilder(TreeBuilderBase treeBuilder, bool expandRoots, CancellationToken cancellation)
		{
			base.AcceptBuilder(treeBuilder, expandRoots, cancellation);
			int eventsCount = Children.OfType<DacEventsGroupingNodeViewModel>()
									  .Sum(dacVM => dacVM.EventsCount);
			if (Children.Count <= 0)
				return;

			Name = $"{CategoryDescription}({eventsCount})";
		}
	}
}
