using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphCodeMapTreeBuilder : TreeBuilderBase
	{
		

		public virtual IEnumerable<TreeNodeViewModel> CreateChildrenNodes(TreeNodeViewModel parentNode)
		{
			parentNode.ThrowOnNull(nameof(parentNode));

			switch (parentNode)
			{
				default:
					return Enumerable.Empty<TreeNodeViewModel>();
			}
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraph(GraphNodeViewModel graphNode)
		{

		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategories(GraphNodeViewModel graphNode)
		{

		}

		
	}
}
