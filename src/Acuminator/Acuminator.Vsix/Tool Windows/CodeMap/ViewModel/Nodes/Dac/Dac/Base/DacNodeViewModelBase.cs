#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacNodeViewModelBase : TreeNodeViewModel, IElementWithTooltip
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public DacNodeViewModelBase(TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
							   base(tree, parent, isExpanded)
		{
		}

		protected virtual IEnumerable<ExtraInfoViewModel> GetDacExtraInfos(DacSemanticModelForCodeMap? dacSemanticModel)
		{
			if (dacSemanticModel == null)
				yield break;

			if (dacSemanticModel.IsProjectionDac)
			{
				yield return new IconViewModel(this, Icon.ProjectionDac);
			}

			yield return CreateDacTypeInfo(dacSemanticModel.DacType == DacType.Dac);

			var pxCacheNameAttributeInfo = dacSemanticModel.Attributes.FirstOrDefault(attrInfo => attrInfo.IsPXCacheName);
			string? dacFriendlyName		 = pxCacheNameAttributeInfo?.AttributeData.GetNameFromPXCacheNameAttribute()
																				  .NullIfWhiteSpace();
			if (dacFriendlyName != null)
			{
				Color color = Color.FromRgb(38, 155, 199);
				dacFriendlyName = $"\"{dacFriendlyName}\"";

				yield return new TextViewModel(this, dacFriendlyName, darkThemeForeground: color, lightThemeForeground: color);
			}
		}

		protected TextViewModel CreateDacTypeInfo(bool isDac)
		{
			Color color = Color.FromRgb(38, 155, 199);

			string dacType = isDac
				? VSIXResource.CodeMap_ExtraInfo_IsDac
				: VSIXResource.CodeMap_ExtraInfo_IsDacExtension;

			return new TextViewModel(this, dacType, darkThemeForeground: color, lightThemeForeground: color);
		}

		TooltipInfo? IElementWithTooltip.CalculateTooltip() => CalculateTooltip();

		protected virtual TooltipInfo? CalculateTooltip()
		{
			var dacAttributesGroupNode = AllChildren.OfType<DacAttributesGroupNodeViewModel>().FirstOrDefault();
			return dacAttributesGroupNode is IElementWithTooltip elementWithTooltip
				? elementWithTooltip.CalculateTooltip()
				: null;
		}
	}
}