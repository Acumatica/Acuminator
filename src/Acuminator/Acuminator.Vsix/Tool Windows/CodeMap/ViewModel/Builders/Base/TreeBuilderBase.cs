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
			var roots = CreateRoots(codeMapTree, expandRoots, cancellation)?.Where(root => root != null).ToList();

			if (roots.IsNullOrEmpty())
				return codeMapTree;
	
			cancellation.ThrowIfCancellationRequested();

			foreach (TreeNodeViewModel root in roots)
			{
				CreateRootTree(root, expandChildren, cancellation);
			}

			var rootsToAdd = roots.Where(root => root.Children.Count > 0 || root.DisplayNodeWithoutChildren);
			codeMapTree.RootItems.AddRange(rootsToAdd);
			return codeMapTree;
		}

		protected virtual void CreateRootTree(TreeNodeViewModel root, bool expandChildren, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var parentWithChildrenStack = new Stack<(TreeNodeViewModel Parent, List<TreeNodeViewModel> Children)>(64);
			var parentNodesStack = new Stack<TreeNodeViewModel>(64);
			parentNodesStack.Push(root);

			//Fill the parentWithChildrenStack with tree structure
			while (parentNodesStack.Count > 0)
			{
				cancellation.ThrowIfCancellationRequested();

				TreeNodeViewModel currentParent = parentNodesStack.Pop();
				var children = GetChildrenNodes(currentParent, expandChildren, cancellation)?.Where(node => node != null).ToList();

				if (children.IsNullOrEmpty())
					continue;

				parentWithChildrenStack.Push((currentParent, children));

				foreach (var child in children)
				{
					parentNodesStack.Push(child);
				}
			}

			//Set the references walking upward tree
			while (parentWithChildrenStack.Count > 0)
			{
				cancellation.ThrowIfCancellationRequested();

				var (parent, children) = parentWithChildrenStack.Pop();
				var childrenToSet = children.Where(c => c.Children.Count > 0 || c.DisplayNodeWithoutChildren);
				parent.Children.Reset(childrenToSet);
			}
		}

		protected abstract IEnumerable<TreeNodeViewModel> GetChildrenNodes(TreeNodeViewModel parentNode, bool expandChildren,
																		   CancellationToken cancellation);

		protected abstract IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, bool expandRoots, CancellationToken cancellation);		
	}
}
