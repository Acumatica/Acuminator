using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract class TreeBuilderBase
	{
		public virtual TreeViewModel CreateEmptyCodeMapTree(CodeMapWindowViewModel windowViewModel) => new TreeViewModel(windowViewModel);

		public TreeViewModel BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, CancellationToken cancellation)
		{
			windowViewModel.ThrowOnNull(nameof(windowViewModel));
			cancellation.ThrowIfCancellationRequested();

			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null)
				return null;

			cancellation.ThrowIfCancellationRequested();
			var roots = CreateRoots(windowViewModel, cancellation)?.Where(root => root != null);

			if (roots.IsNullOrEmpty())
				return codeMapTree;

			codeMapTree.RootItems.AddRange(roots);
			cancellation.ThrowIfCancellationRequested();

			foreach (TreeNodeViewModel root in codeMapTree.RootItems)
			{
				var rootNodes = CreateChildrenNodes(root, cancellation)?.Where(node => node != null);

				if (!rootNodes.IsNullOrEmpty())
				{
					root.Children.AddRange(rootNodes);
				}

				cancellation.ThrowIfCancellationRequested();
			}

			return codeMapTree;
		}

		protected abstract IEnumerable<TreeNodeViewModel> CreateRoots(CodeMapWindowViewModel windowViewModel, CancellationToken cancellation);

		public abstract IEnumerable<TreeNodeViewModel> CreateChildrenNodes(TreeNodeViewModel parentNode, CancellationToken cancellation);
	}
}
