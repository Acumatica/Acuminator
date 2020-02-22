using Acuminator.Utilities.Common;
using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Information for sort of code map tree
	/// </summary>
	public class CodeMapSortContext
	{
		public SortType SortType 
		{ 
			get;
			set;
		}

		public SortDirection SortDirection 
		{ 
			get;
			set;
		}

		public bool SortDescendants 
		{ 
			get;
			set;
		}

		public CodeMapSortContext(SortType sortType, SortDirection sortDirection, bool sortDescendants)
		{
			SortType = sortType;
			SortDirection = sortDirection;
			SortDescendants = sortDescendants;
		}
	}
}
