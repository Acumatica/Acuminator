using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public RowEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.RowEvent, isExpanded)
		{
			
		}

		protected override void AddCategoryMembers() =>
			AddCategoryMembersDefaultImpl(graph =>
				graph.RowInsertingEvents
					 .Concat(graph.RowInsertedEvents)
					 .Concat(graph.RowSelectingEvents)
					 .Concat(graph.RowSelectedEvents)
					 .Concat(graph.RowUpdatingEvents)
					 .Concat(graph.RowUpdatedEvents)
					 .Concat(graph.RowDeletingEvents)
					 .Concat(graph.RowDeletedEvents)
					 .Concat(graph.RowPersistingEvents)
					 .Concat(graph.RowPersistedEvents));
	}
}
