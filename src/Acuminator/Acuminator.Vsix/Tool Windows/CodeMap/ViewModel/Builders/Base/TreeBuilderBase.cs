#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract partial class TreeBuilderBase : CodeMapTreeVisitor<IEnumerable<TreeNodeViewModel>?>
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

		protected TreeBuilderBase() : base([])
		{
		}

		public virtual TreeViewModel CreateEmptyCodeMapTree(CodeMapWindowViewModel windowViewModel) => new TreeViewModel(windowViewModel);

		public TreeViewModel? BuildCodeMapTreeForCustomSemanticModel(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels, 
																	 FilterOptions? filterOptions, bool expandRoots, bool expandChildren, 
																	 CancellationToken cancellation) =>
			BuildCodeMapTree(windowViewModel, semanticModels, filterOptions, expandRoots, expandChildren, cancellation);

		public TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, FilterOptions? filterOptions, bool expandRoots,
											   bool expandChildren, CancellationToken cancellation) =>
			BuildCodeMapTree(windowViewModel, semanticModels: windowViewModel.DocumentModel?.CodeMapSemanticModels, 
							 filterOptions, expandRoots, expandChildren, cancellation);

		private TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels,
												FilterOptions? filterOptions, bool expandRoots, bool expandChildren, CancellationToken cancellation)
		{
			windowViewModel.ThrowOnNull();
			filterOptions ??= FilterOptions.NoFilter;

			try
			{
				Cancellation = cancellation;
				return BuildCodeMapTree(windowViewModel, semanticModels, filterOptions, expandRoots, expandChildren);
			}
			finally
			{
				Cancellation = CancellationToken.None;
			}
		}

		protected TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels, 
												  FilterOptions filterOptions, bool expandRoots, bool expandChildren)
		{
			Cancellation.ThrowIfCancellationRequested();
			
			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null || semanticModels.IsNullOrEmpty())
				return null;

			Cancellation.ThrowIfCancellationRequested();
			List<TreeNodeViewModel> roots;

			try
			{
				ExpandCreatedNodes = expandRoots;
				roots = CreateRoots(codeMapTree, semanticModels).Where(root => root != null).ToList(capacity: 4);
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

			var rootsToAdd = roots.Where(root => root.AllChildren.Count > 0 || ShouldAddNodeWithoutChildrenToTree(root));

			codeMapTree.FillCodeMapTree(rootsToAdd, filterOptions);
			return codeMapTree;
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, IEnumerable<ISemanticModel> semanticModels)
		{
			if (tree.CodeMapViewModel.DocumentModel == null)
				yield break;

			foreach (ISemanticModel rootSemanticModel in semanticModels)
			{
				Cancellation.ThrowIfCancellationRequested();
				TreeNodeViewModel? rootVM = CreateRoot(rootSemanticModel, tree);

				if (rootVM != null)
					yield return rootVM;
			}
		}

		protected abstract TreeNodeViewModel? CreateRoot(ISemanticModel rootSemanticModel, TreeViewModel tree);

		protected virtual void BuildSubTree(TreeNodeViewModel subtreeRoot)
		{
			var generatedChildrenSeq = VisitNode(subtreeRoot);
			List<TreeNodeViewModel>? children = (generatedChildrenSeq as List<TreeNodeViewModel>) ?? generatedChildrenSeq?.ToList();

			if (children.IsNullOrEmpty())
				return;

			foreach (TreeNodeViewModel? child in children)
			{
				if (child != null)
					BuildSubTree(child);
			}

			var childrenToAdd = children.Where(c => c != null && (c.AllChildren.Count > 0 || ShouldAddNodeWithoutChildrenToTree(c)));

			subtreeRoot.AllChildren.Reset(childrenToAdd);
		}

		protected virtual bool ShouldAddNodeWithoutChildrenToTree(TreeNodeViewModel node) => node switch
		{
			AttributesGroupNodeViewModel 	  => false,
			AttributeNodeViewModel 			  => true,
			GraphMemberCategoryNodeViewModel  => false,
			DacMemberCategoryNodeViewModel 	  => false,
			DacMemberNodeViewModel 			  => true,
			GraphMemberNodeViewModel 		  => true,
			GraphMemberInfoNodeViewModel 	  => true,
			DacGroupingNodeBaseViewModel 	  => false,
			DacFieldGroupingNodeBaseViewModel => false,
			DacFieldNodeViewModel 			  => false,
			GraphNodeViewModel 				  => true,
			DacNodeViewModel 				  => true,
			BaseDacNodeViewModel			  => true,
			_ 								  => throw new NotImplementedException($"Nodes of type \"{node.GetType().Name}\" are not supported")
		};
	}
}
