#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class BaseDacPlaceholderNodeViewModel : DacNodeViewModelBase
	{
		public DacNodeViewModel ContainingDacNode { get; }

		public DacSemanticModelForCodeMap ParentDacModel => ContainingDacNode.DacModelForCodeMap;

		public override DacOrDacExtInfoBase DacOrDacExtInfo { get; }

		public override bool IsExpanderAlwaysVisible => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public BaseDacPlaceholderNodeViewModel(DacOrDacExtInfoBase dacOrDacExtInfo, DacNodeViewModel containingDacNode, 
											   TreeNodeViewModel parent, bool isExpanded) : 
										base(containingDacNode.CheckIfNull().Tree, parent, isExpanded)
		{
			DacOrDacExtInfo	  = dacOrDacExtInfo.CheckIfNull();
			ContainingDacNode = containingDacNode;

			var dacTypeInfo = CreateDacTypeInfo();
			ExtraInfos		= new ExtendedObservableCollection<ExtraInfoViewModel>(dacTypeInfo);
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) =>
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		protected override bool BeforeNodeExpansionChanged(bool oldValue, bool newValue)
		{
			base.BeforeNodeExpansionChanged(oldValue, newValue);

			bool isExpanding = newValue;

			if (!isExpanding || Tree.CodeMapViewModel.IsCalculating)
				return false;

			bool oldIsCalculating = Tree.CodeMapViewModel.IsCalculating;

			try
			{
				Tree.CodeMapViewModel.IsCalculating = true;
				BuildBaseDacNodes();

				// Never expand substitute node
				return false;
			}
			finally
			{
				Tree.CodeMapViewModel.IsCalculating = oldIsCalculating;
			}
		}

		private void BuildBaseDacNodes()
		{
			var pxContext = ContainingDacNode.DacModel.PXContext;
			var semanticModelFactory = Tree.CodeMapViewModel.SemanticModelFactory;

			if (!semanticModelFactory.TryToInferSemanticModel(DacOrDacExtInfo.Symbol, pxContext, out ISemanticModel? semanticModel, 
															 DacOrDacExtInfo.DeclarationOrder) ||
				semanticModel is not DacSemanticModelForCodeMap baseDacSemanticModel)
			{
				return;
			}

			var treeBuilder   = Tree.CodeMapViewModel.TreeBuilder;
			var filterOptions = Tree.CodeMapViewModel.FilterVM.CreateFilterOptionsFromCurrentFilter();
			var rootParent	  = Parent;

			if (rootParent == null)
				return;

			var baseDacNode = treeBuilder.CreateAttachedRootWithSubTree(baseDacSemanticModel, Tree, rootParent, filterOptions, 
																		expandRoots: true, expandChildren: false,
																		Tree.CodeMapViewModel.CancellationToken ?? default) as DacNodeViewModel;
			if (baseDacNode == null)
				return;

			int nodeIndex = rootParent.AllChildren.IndexOf(this);

			if (nodeIndex >= 0)
			{
				rootParent.AllChildren.RemoveAt(nodeIndex);
				rootParent.AllChildren.Insert(nodeIndex, baseDacNode);
			}
			else
			{
				rootParent.AllChildren.Remove(this);
				rootParent.AllChildren.Add(baseDacNode);
			}

			Tree.RefreshFlattenedNodesList();
		}
	}
}