using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A Code Map tree nodes special sorter implementation used to sort tree during tree construction.
	/// </summary>
	public class CodeMapTreeInitialSorter : CodeMapTreeSorterBase
	{
		public CodeMapTreeInitialSorter(SortType defaultSortType, SortDirection defaultSortDirection)
		{
			SortContext = new CodeMapSortContext(defaultSortType, defaultSortDirection, sortDescendants: false);
		}

		public override void VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode)
		{
			base.VisitNode(dacGroupingNode);
		}

		public override void VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode)
		{
			try
			{
				SortContext.SortType = SortType.Alphabet;
				base.VisitNode(dacGroupingNode);
			}
			finally
			{

			}		
		}


		public override void VisitNode(AttributeNodeViewModel attributeNode)
		{
			//Stop visit for better performance
		}
	}
}