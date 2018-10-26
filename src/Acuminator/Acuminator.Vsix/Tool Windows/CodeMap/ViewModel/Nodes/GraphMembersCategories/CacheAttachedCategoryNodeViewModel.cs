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
	public class CacheAttachedCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public CacheAttachedCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) :
									base(graphViewModel, GraphMemberType.CacheAttached, isExpanded)
		{

		}

		protected override void AddCategoryMembers() =>
			AddCategoryMembersDefaultImpl(graph => graph.CacheAttachedEvents);
	}
}
