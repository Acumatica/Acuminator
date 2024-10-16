#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacNodeViewModel : DacNodeViewModelBase, IElementWithTooltip
	{
		public DacSemanticModelForCodeMap DacModelForCodeMap { get; }

		public DacSemanticModel DacModel => DacModelForCodeMap.DacModel;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public override DacOrDacExtInfoBase DacOrDacExtInfo => DacModel.DacOrDacExtInfo;

		public DacNodeViewModel(DacSemanticModelForCodeMap dacModel, TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
						   base(tree, parent, isExpanded)
		{
			DacModelForCodeMap = dacModel.CheckIfNull();
			ExtraInfos		   = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetDacExtraInfos()
		{
			if (DacModelForCodeMap.IsProjectionDac)
			{
				yield return new IconViewModel(this, Icon.ProjectionDac);
			}

			yield return CreateDacTypeInfo();

			var pxCacheNameAttributeInfo = DacModelForCodeMap.Attributes.FirstOrDefault(attrInfo => attrInfo.IsPXCacheName);
			string? dacFriendlyName = pxCacheNameAttributeInfo?.AttributeData.GetNameFromPXCacheNameAttribute()
																			 .NullIfWhiteSpace();
			if (dacFriendlyName != null)
			{
				Color color = Color.FromRgb(38, 155, 199);
				dacFriendlyName = $"\"{dacFriendlyName}\"";

				yield return new TextViewModel(this, dacFriendlyName, darkThemeForeground: color, lightThemeForeground: color);
			}
		}

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var dacAttributesGroupNode = AllChildren.OfType<DacAttributesGroupNodeViewModel>().FirstOrDefault();
			return dacAttributesGroupNode is IElementWithTooltip elementWithTooltip
				? elementWithTooltip.CalculateTooltip()
				: null;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}