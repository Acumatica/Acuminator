#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal static class PlaceholderNodeUtils
	{
		public static bool ReplacePlaceholderWithSubTreeOnExpansion<TNode>(this TNode placeholderNode, PXContext pxContext, bool isExpanding)
		where TNode : TreeNodeViewModel, IPlaceholderNode
		{
			pxContext.ThrowOnNull();

			var tree = placeholderNode.Tree;

			if (!isExpanding || placeholderNode == null || tree.CodeMapViewModel.IsCalculating)
				return false;

			bool oldIsCalculating = tree.CodeMapViewModel.IsCalculating;

			try
			{
				tree.CodeMapViewModel.IsCalculating = true;
				var cancellation = tree.CodeMapViewModel.CancellationToken ?? AcuminatorVSPackage.Instance.DisposalToken;

				BuildRealNode(placeholderNode, pxContext, cancellation);

				// Never expand substitute node
				return false;
			}
			finally
			{
				tree.CodeMapViewModel.IsCalculating = oldIsCalculating;
			}
		}

		private static void BuildRealNode<TNode>(TNode placeholderNode, PXContext pxContext, CancellationToken cancellation)
		where TNode : TreeNodeViewModel, IPlaceholderNode
		{
			var tree = placeholderNode.Tree;
			var semanticModelFactory = tree.CodeMapViewModel.SemanticModelFactory;

			if (!semanticModelFactory.TryToInferSemanticModel(placeholderNode.PlaceholderSymbol, pxContext, out ISemanticModel? semanticModel,
															  placeholderNode.PlaceholderSymbolDeclarationOrder, cancellation) ||
				semanticModel == null)
			{
				return;
			}

			var treeBuilder   = tree.CodeMapViewModel.TreeBuilder;
			var filterOptions = tree.CodeMapViewModel.FilterVM.CreateFilterOptionsFromCurrentFilter();
			var rootParent 	  = placeholderNode.Parent;

			if (rootParent == null)
				return;

			var subTreeRoot = treeBuilder.CreateAttachedRootWithSubTree(semanticModel, tree, rootParent, filterOptions,
																		expandRoots: true, expandChildren: false, cancellation);
			if (subTreeRoot == null)
				return;

			int placeholderIndex = rootParent.AllChildren.IndexOf(placeholderNode);

			if (placeholderIndex >= 0)
			{
				rootParent.AllChildren.RemoveAt(placeholderIndex);
				rootParent.AllChildren.Insert(placeholderIndex, subTreeRoot);
			}
			else
			{
				rootParent.AllChildren.Remove(placeholderNode);
				rootParent.AllChildren.Add(subTreeRoot);
			}

			tree.RefreshFlattenedNodesList();
		}
	}
}
