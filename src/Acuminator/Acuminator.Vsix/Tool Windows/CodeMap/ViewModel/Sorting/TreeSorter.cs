using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TreeSorter
	{
		private readonly IComparer<TreeNodeViewModel> _comparer;

		public TreeSorter(IComparer<TreeNodeViewModel> comparer)
		{
			_comparer = comparer.CheckIfNull(nameof(comparer));
		}

		public virtual void SortChildren(TreeNodeViewModel treeNode)
		{
			switch (treeNode)
			{

				case null:
					return;
				default:
					return;
			}
		}
	}
}