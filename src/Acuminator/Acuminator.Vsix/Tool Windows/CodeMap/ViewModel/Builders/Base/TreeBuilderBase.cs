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
		protected CancellationToken Cancellation 
		{ 
			get;
			private set;
		} = CancellationToken.None;

		protected ExpandCreatedNodesFunction ExpandCreatedNodesCalculator { get; }

		protected TreeBuilderBase() : base([])
		{
			ExpandCreatedNodesCalculator = ExpandCreatedNodeCalculation;
		}

		protected virtual bool ExpandCreatedNodeCalculation(TreeNodeViewModel treeNode, bool expandAttributeNodes, bool expandRegularNodes, 
															bool expandRootNotes) =>
			treeNode switch
			{
				AttributesGroupNodeViewModel 	  => expandAttributeNodes,
				AttributeNodeViewModel 			  => expandAttributeNodes,
				CacheAttachedNodeViewModel		  => expandAttributeNodes,
				NonBqlDacPropertyNodeViewModel	  => expandAttributeNodes,
				DacFieldPropertyNodeViewModel	  => expandAttributeNodes,

				DacNodeViewModel 				  => expandRootNotes,
				GraphNodeViewModel 				  => expandRootNotes,

				BaseDacPlaceholderNodeViewModel   => false,
				BaseGraphPlaceholderNodeViewModel => false,
				_ 								  => expandRegularNodes
			};

		public virtual TreeViewModel CreateEmptyCodeMapTree(CodeMapWindowViewModel windowViewModel) => new TreeViewModel(windowViewModel);

		public TreeViewModel? BuildCodeMapTreeForCustomSemanticModel(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels,
																	 FilterOptions? filterOptions, CancellationToken cancellation) =>
			BuildCodeMapTree(windowViewModel, semanticModels, filterOptions, cancellation);

		public TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, FilterOptions? filterOptions, CancellationToken cancellation) =>
			BuildCodeMapTree(windowViewModel, semanticModels: windowViewModel.DocumentModel?.CodeMapSemanticModels, filterOptions, cancellation);

		private TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels,
												FilterOptions? filterOptions, CancellationToken cancellation)
		{
			windowViewModel.ThrowOnNull();
			filterOptions ??= FilterOptions.NoFilter;

			try
			{
				Cancellation = cancellation;
				return BuildCodeMapTree(windowViewModel, semanticModels, filterOptions);
			}
			finally
			{
				Cancellation = CancellationToken.None;
			}
		}

		protected TreeViewModel? BuildCodeMapTree(CodeMapWindowViewModel windowViewModel, IReadOnlyCollection<ISemanticModel>? semanticModels,
												  FilterOptions filterOptions)
		{
			Cancellation.ThrowIfCancellationRequested();
			
			TreeViewModel codeMapTree = CreateEmptyCodeMapTree(windowViewModel);

			if (codeMapTree == null || semanticModels.IsNullOrEmpty())
				return null;

			Cancellation.ThrowIfCancellationRequested();

			List<TreeNodeViewModel> roots = CreateRoots(codeMapTree, semanticModels).Where(root => root != null)
																					.ToList(capacity: 4);
			if (roots.IsNullOrEmpty())
				return codeMapTree;

			Cancellation.ThrowIfCancellationRequested();

			foreach (TreeNodeViewModel root in roots)
			{
				BuildSubTree(root);
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
				TreeNodeViewModel? rootVM = CreateRoot(rootSemanticModel, parent: null, tree);

				if (rootVM != null)
					yield return rootVM;
			}
		}

		/// <summary>
		/// Creates root node with a built subtree of descendants attached to a <paramref name="rootParent"/> node of existing tree.<br/>
		/// The root won't be added to <paramref name="tree"/> roots.
		/// </summary>
		/// <param name="rootSemanticModel">The root semantic model.</param>
		/// <param name="tree">The custom tree, reference to which will be set in all nodes.</param>
		/// <param name="rootParent">(Optional) The root parent.</param>
		/// <param name="filterOptions">(Optional)Options for controlling the filter.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// New separate root with built sub-tree.
		/// </returns>
		public TreeNodeViewModel? CreateAttachedRootWithSubTree(ISemanticModel rootSemanticModel, TreeViewModel tree, TreeNodeViewModel? rootParent,
																FilterOptions? filterOptions, CancellationToken cancellation)
		{
			rootSemanticModel.ThrowOnNull();
			tree.ThrowOnNull();

			filterOptions ??= FilterOptions.NoFilter;

			try
			{
				Cancellation = cancellation;
				var root = CreateStandAloneRootWithSubTree(rootSemanticModel, tree, rootParent, filterOptions);

				return root;
			}
			finally
			{
				Cancellation = CancellationToken.None;
			}
		}

		private TreeNodeViewModel? CreateStandAloneRootWithSubTree(ISemanticModel rootSemanticModel, TreeViewModel tree, TreeNodeViewModel? parent,
																   FilterOptions filterOptions)
		{
			TreeNodeViewModel? rootNode = CreateRoot(rootSemanticModel, parent, tree);

			if (rootNode == null)
				return null;

			BuildSubTree(rootNode);

			if (rootNode.AllChildren.Count == 0 && !ShouldAddNodeWithoutChildrenToTree(rootNode))
				return null;

			rootNode.RefreshVisibilityForNodeAndSubTreeFromFilter(filterOptions);
			return rootNode;
		}

		protected abstract TreeNodeViewModel? CreateRoot(ISemanticModel rootSemanticModel, TreeNodeViewModel? parent, TreeViewModel tree);

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
			DacFieldNodeViewModelBase 		  => false,
			GraphNodeViewModel 				  => true,
			DacNodeViewModel 				  => true,
			BaseDacPlaceholderNodeViewModel	  => true,
			BaseGraphPlaceholderNodeViewModel => true,
			_ 								  => throw new NotImplementedException($"Nodes of type \"{node.GetType().Name}\" are not supported")
		};
	}
}
