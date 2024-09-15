#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacFieldCategoryNodeViewModel : DacMemberCategoryNodeViewModel
	{
		protected DacFieldCategoryNodeViewModel(DacNodeViewModel dacViewModel, TreeNodeViewModel parent, DacMemberCategory dacCategoryType, bool isExpanded) : 
										   base(dacViewModel, parent, dacCategoryType, isExpanded)
		{}

		public abstract IEnumerable<DacFieldInfo> GetCategoryDacFields();
	}
}