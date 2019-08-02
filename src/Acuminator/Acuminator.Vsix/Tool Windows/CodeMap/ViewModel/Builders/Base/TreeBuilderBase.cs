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

		public TreeViewModel BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, bool expandRoots, bool expandChildren, CancellationToken cancellation)
		{
			windowViewModel.ThrowOnNull(nameof(windowViewModel));
			cancellation.ThrowIfCancellationRequested();

			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null)
				return null;

			cancellation.ThrowIfCancellationRequested();
			var roots = CreateRoots(codeMapTree, expandRoots, cancellation)?.Where(root => root != null);

			if (roots.IsNullOrEmpty())
				return codeMapTree;

			codeMapTree.RootItems.AddRange(roots);
			cancellation.ThrowIfCancellationRequested();

			foreach (TreeNodeViewModel root in codeMapTree.RootItems)
			{
				CreateAndFillChildrenNodes(root, expandChildren, cancellation);
			}

			return codeMapTree;
		}

		protected void CreateAndFillChildrenNodes(TreeNodeViewModel parentNode, bool expandChildren, CancellationToken cancellation)
		{
			var parentsStack = new Stack<TreeNodeViewModel>(capacity: 32);
			parentsStack.Push(parentNode);

			while (parentsStack.Count > 0)
			{
				cancellation.ThrowIfCancellationRequested();

				TreeNodeViewModel currentParent = parentsStack.Pop();
				var children = CreateChildrenNodes(currentParent, expandChildren, cancellation)?.Where(node => node != null);

				if (children == null)
					continue;

				currentParent.Children.AddRange(children);

				foreach (TreeNodeViewModel child in currentParent.Children)
				{		
					parentsStack.Push(child);				
				}
			}
		}

		protected abstract IEnumerable<TreeNodeViewModel> CreateChildrenNodes(TreeNodeViewModel parentNode, bool expandChildren,
																			  CancellationToken cancellation);

		protected abstract IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, bool expandRoots, CancellationToken cancellation);		
	}
}
