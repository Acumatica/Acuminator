using Acuminator.Utilities.Common;
using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Information for sort of code map tree
	/// </summary>
	public struct CodeMapSortContext
	{
		public SortType SortType { get; }

		public SortDirection SortDirection { get; }

		public bool SortDescendants { get; }

		public CodeMapSortContext(SortType sortType, SortDirection sortDirection, bool sortDescendants)
		{
			SortType = sortType;
			SortDirection = sortDirection;
			SortDescendants = sortDescendants;
		}
	}
}
