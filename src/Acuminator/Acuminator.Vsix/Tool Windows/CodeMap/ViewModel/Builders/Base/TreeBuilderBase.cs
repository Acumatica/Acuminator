using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract class TreeBuilderBase
	{
		public virtual TreeViewModel CreateEmptyCodeMapTree(CodeMapWindowViewModel windowViewModel) => new TreeViewModel(windowViewModel);

		public TreeViewModel BuildCodeMapTree(CodeMapWindowViewModel windowViewModel)
		{
			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null)
				return null;

			var roots = CreateRoots()?.Where(root => root != null);

			if (roots.IsNullOrEmpty())
				return codeMapTree;

			codeMapTree.RootItems.AddRange(roots);

			foreach (TreeNodeViewModel root in codeMapTree.RootItems)
			{
				var rootNodes = CreateChildrenNodes(root)?.Where(node => node != null);

				if (!rootNodes.IsNullOrEmpty())
				{
					root.Children.AddRange(rootNodes);
				}
			}

			return codeMapTree;
		}

		protected abstract IEnumerable<TreeNodeViewModel> CreateRoots();

		public abstract IEnumerable<TreeNodeViewModel> CreateChildrenNodes(TreeNodeViewModel parentNode);
	}
}
