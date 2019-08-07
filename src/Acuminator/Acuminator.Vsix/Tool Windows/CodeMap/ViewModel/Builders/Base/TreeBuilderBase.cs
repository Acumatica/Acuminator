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
	public abstract partial class TreeBuilderBase
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
				root.AcceptBuilder(this, expandChildren, cancellation);
			}

			var rootsToAdd = roots.Where(root => root.Children.Count > 0 || root.DisplayNodeWithoutChildren);
			codeMapTree.RootItems.AddRange(rootsToAdd);
			return codeMapTree;
		}

		protected abstract IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, bool expandRoots, CancellationToken cancellation);

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(AttributeNodeViewModel attributeNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
	}
}
