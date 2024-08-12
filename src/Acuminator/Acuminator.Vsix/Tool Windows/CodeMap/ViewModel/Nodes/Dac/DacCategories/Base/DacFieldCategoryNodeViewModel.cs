#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacFieldCategoryNodeViewModel : DacMemberCategoryNodeViewModel
	{
		protected DacFieldCategoryNodeViewModel(DacNodeViewModel dacViewModel, DacMemberCategory dacCategoryType, bool isExpanded) : 
										   base(dacViewModel, dacCategoryType, isExpanded)
		{}

		public abstract IEnumerable<DacFieldInfo> GetCategoryDacFields();
	}
}