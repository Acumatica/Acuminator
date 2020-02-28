using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract partial class TreeBuilderBase : CodeMapTreeVisitor<IEnumerable<TreeNodeViewModel>>
	{
		protected bool ExpandCreatedNodes 
		{ 
			get;
			set;
		}

		protected CancellationToken Cancellation 
		{ 
			get;
			private set;
		} = CancellationToken.None;

		protected TreeBuilderBase() : base(Enumerable.Empty<TreeNodeViewModel>())
		{
		}

		public virtual TreeViewModel CreateEmptyCodeMapTree(CodeMapWindowViewModel windowViewModel) => new TreeViewModel(windowViewModel);

		public TreeViewModel BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, bool expandRoots, bool expandChildren, CancellationToken cancellation)
		{
			windowViewModel.ThrowOnNull(nameof(windowViewModel));

			try
			{
				Cancellation = cancellation;
				return BuildCodeMapTree(windowViewModel, expandRoots, expandChildren);
			}
			finally
			{
				Cancellation = CancellationToken.None;
			}		
		}

		protected TreeViewModel BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, bool expandRoots, bool expandChildren)
		{
			Cancellation.ThrowIfCancellationRequested();

			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null)
				return null;

			Cancellation.ThrowIfCancellationRequested();
			List<TreeNodeViewModel> roots;

			try
			{
				ExpandCreatedNodes = expandRoots;
				roots = CreateRoots(codeMapTree)?.Where(root => root != null).ToList();
			}
			finally
			{
				ExpandCreatedNodes = false;
			}

			if (roots.IsNullOrEmpty())
				return codeMapTree;

			Cancellation.ThrowIfCancellationRequested();

			try
			{
				ExpandCreatedNodes = expandChildren;

				foreach (TreeNodeViewModel root in roots)
				{
					BuildSubTree(root);
				}
			}
			finally
			{
				ExpandCreatedNodes = false;
			}

			var rootsToAdd = roots.Where(root => root.Children.Count > 0 || root.DisplayNodeWithoutChildren);

			codeMapTree.FillCodeMapTree(rootsToAdd);
			return codeMapTree;
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree)
		{
			if (tree.CodeMapViewModel.DocumentModel == null)
				yield break;

			foreach (ISemanticModel rootSemanticModel in tree.CodeMapViewModel.DocumentModel.CodeMapSemanticModels)
			{
				Cancellation.ThrowIfCancellationRequested();
				TreeNodeViewModel rootVM = CreateRoot(rootSemanticModel, tree);

				if (rootVM != null)
				{
					yield return rootVM;
				}
			}
		}

		protected abstract TreeNodeViewModel CreateRoot(ISemanticModel rootSemanticModel, TreeViewModel tree);

		protected virtual void BuildSubTree(TreeNodeViewModel subtreeRoot)
		{
			var generatedChildrenSeq = VisitNode(subtreeRoot);
			var children = (generatedChildrenSeq as List<TreeNodeViewModel>) ?? generatedChildrenSeq?.ToList();

			if (children.IsNullOrEmpty())
				return;

			foreach (var child in children)
			{
				BuildSubTree(child);
			}

			var childrenToAdd = children.Where(c => c != null && (c.Children.Count > 0 || c.DisplayNodeWithoutChildren));

			subtreeRoot.Children.Reset(childrenToAdd);
		}
	}
}
