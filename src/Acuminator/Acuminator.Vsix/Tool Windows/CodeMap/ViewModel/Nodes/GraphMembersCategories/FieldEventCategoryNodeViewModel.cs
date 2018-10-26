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
	public class FieldEventCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		public FieldEventCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.FieldEvent, isExpanded)
		{
			
		}

		protected override void AddCategoryMembers()
		{

		}
	}
}
