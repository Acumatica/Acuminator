using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes default sorter implementation.
	/// </summary>
	public class CodeMapTreeSorterDefault : CodeMapTreeSorterBase
	{
		protected override bool IsSortTypeSupported(TreeNodeViewModel node, SortType sortType)
		{
			switch (node)
			{
				case DacGroupingNodeBaseViewModel _:
				case DacFieldGroupingNodeBaseViewModel _:
					return sortType == SortType.Alphabet;
				case DacMemberNodeViewModel _:
					return sortType == SortType.Alphabet || sortType == SortType.Declaration;
				default:
					return base.IsSortTypeSupported(node, sortType);
			}
		}

		public override void VisitNode(AttributeNodeViewModel attributeNode)
		{
			//Stop visit for better performance
		}
	}
}