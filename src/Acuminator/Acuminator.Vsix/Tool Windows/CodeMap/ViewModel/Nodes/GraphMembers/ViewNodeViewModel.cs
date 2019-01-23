using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ViewNodeViewModel : GraphMemberNodeViewModel
	{
		public ViewNodeViewModel(ViewCategoryNodeViewModel viewCategoryVM, DataViewInfo viewInfo, bool isExpanded = false) :
							base(viewCategoryVM, viewInfo, isExpanded)
		{
			AddViewDelegate();
		}	

		protected virtual void AddViewDelegate()
		{
			if (MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(MemberSymbol.Name, out DataViewDelegateInfo viewDelegate))
			{
				Children.Add(new GraphMemberNodeViewModel(MemberCategory, viewDelegate));
			}
		}
	}
}
