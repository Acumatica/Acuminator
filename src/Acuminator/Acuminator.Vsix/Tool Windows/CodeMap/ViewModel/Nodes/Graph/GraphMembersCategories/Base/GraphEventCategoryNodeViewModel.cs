#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;

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
			SubscribeOnDisplayedChildrenCollectionChanged(DisplayedChildren_CollectionChanged);
		}

		private void DisplayedChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || 
				e.Action == NotifyCollectionChangedAction.Reset)
			{
				int eventsCount = DisplayedChildren.OfType<DacGroupingNodeBaseViewModel>().Sum(dacVM => dacVM.EventsCount);

				if (DisplayedChildren.Count <= 0)
					return;

				Name = $"{CategoryDescription}({eventsCount})";
			}
		}
	}
}
