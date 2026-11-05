#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class BaseDacPlaceholderNodeViewModel : DacNodeViewModelBase, IPlaceholderNode, IElementWithTooltip
	{
		private readonly Lazy<TooltipInfo?> _tooltipLazy;

		public DacNodeViewModel ContainingDacNode { get; }

		public DacSemanticModelForCodeMap ParentDacModel => ContainingDacNode.DacModelForCodeMap;

		public override DacOrDacExtInfoBase DacOrDacExtInfo { get; }

		public override bool IsExpanderAlwaysVisible => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		#region IPlaceholderNode implementation
		ITypeSymbol IPlaceholderNode.PlaceholderSymbol => DacOrDacExtInfo.Symbol;

		int IPlaceholderNode.PlaceholderSymbolDeclarationOrder => DacOrDacExtInfo.DeclarationOrder;
		#endregion

		public BaseDacPlaceholderNodeViewModel(DacOrDacExtInfoBase dacOrDacExtInfo, DacNodeViewModel containingDacNode, 
											   TreeNodeViewModel parent, ExpandCreatedNodesFunction isExpandedCalculator) : 
										base(containingDacNode.CheckIfNull().Tree, parent, isExpandedCalculator)
		{
			DacOrDacExtInfo	  = dacOrDacExtInfo.CheckIfNull();
			ContainingDacNode = containingDacNode;

			var extraDacInfos = CreateExtraInfos();
			ExtraInfos		  = new ExtendedObservableCollection<ExtraInfoViewModel>(extraDacInfos);

			_tooltipLazy = new Lazy<TooltipInfo?>(CalculateTooltipFromAttributes);
		}

		private IEnumerable<ExtraInfoViewModel> CreateExtraInfos()
		{
			yield return CreateDacTypeInfo();

			if (HasPXAccumulatorAttribute())
			{
				yield return new IconViewModel(this, Icon.PXAccumulatorDac);
			}
		}

		private bool HasPXAccumulatorAttribute()
		{
			var attributes = DacOrDacExtInfo.Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return false;

			var pxContext = ContainingDacNode.DacModel.PXContext;
			var pxAccumulatorAttribute = pxContext.AttributeTypes.PXAccumulatorAttribute;

			if (pxAccumulatorAttribute == null)
				return false;

			bool hasPXAccumulator = attributes.Any(attr => attr.AttributeClass?.InheritsFromOrEquals(pxAccumulatorAttribute) == true);
			return hasPXAccumulator;
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

		TooltipInfo? IElementWithTooltip.CalculateTooltip() => _tooltipLazy.Value;

		private TooltipInfo? CalculateTooltipFromAttributes()
		{
			var attributes = DacOrDacExtInfo.Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return null;
			
			string aggregatedTooltip = attributes.Select(attributeData => $"[{attributeData.ToString().RemoveCommonAcumaticaNamespacePrefixes()}]")
												 .Join(Environment.NewLine);
			return aggregatedTooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(aggregatedTooltip);
		}
	}
}