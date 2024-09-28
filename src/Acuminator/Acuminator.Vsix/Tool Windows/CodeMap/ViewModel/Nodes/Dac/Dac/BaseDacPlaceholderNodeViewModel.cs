#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class BaseDacPlaceholderNodeViewModel : DacNodeViewModelBase, IPlaceholderNode
	{
		public DacNodeViewModel ContainingDacNode { get; }

		public DacSemanticModelForCodeMap ParentDacModel => ContainingDacNode.DacModelForCodeMap;

		public override DacOrDacExtInfoBase DacOrDacExtInfo { get; }

		public override bool IsExpanderAlwaysVisible => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		#region IPlaceholderNode implementation
		INamedTypeSymbol IPlaceholderNode.PlaceholderSymbol => DacOrDacExtInfo.Symbol;

		int IPlaceholderNode.PlaceholderSymbolDeclarationOrder => DacOrDacExtInfo.DeclarationOrder;
		#endregion

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

			return this.ReplacePlaceholderWithSubTreeOnExpansion(ContainingDacNode.DacModel.PXContext, isExpanding: newValue);
		}
	}
}