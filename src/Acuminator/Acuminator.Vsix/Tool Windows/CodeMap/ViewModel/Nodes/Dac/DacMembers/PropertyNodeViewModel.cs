using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PropertyNodeViewModel : DacMemberNodeViewModel, IElementWithTooltip
	{
		public override Icon NodeIcon => IsKey
				? Icon.DacKeyProperty
				: Icon.DacProperty;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public DacPropertyInfo PropertyInfo => MemberInfo as DacPropertyInfo;

		public bool IsDacProperty => PropertyInfo.IsDacProperty;

		public bool IsKey => PropertyInfo.IsKey;

		public bool IsIdentity => PropertyInfo.IsIdentity;

		public BoundType EffectiveBoundType => PropertyInfo.EffectiveBoundType;

		public PropertyNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, DacPropertyInfo propertyInfo, bool isExpanded = false) :
								base(dacMemberCategoryVM, dacMemberCategoryVM, propertyInfo, isExpanded)
		{
			var extraInfos = GetExtraInfos();
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(extraInfos);
		}	

		private IEnumerable<ExtraInfoViewModel> GetExtraInfos()
		{
			yield return new TextViewModel(this, PropertyInfo.EffectivePropertyType.GetSimplifiedName(), 
											darkThemeForeground: Color.FromRgb(86, 156, 214),
											lightThemeForeground: Color.FromRgb(0, 0, 255));
			if (IsIdentity)
			{
				yield return new TextViewModel(this, "ID",
											   darkThemeForeground: Coloriser.VSColors.DacFieldFormatColorDark,
											   lightThemeForeground: Coloriser.VSColors.DacFieldFormatColorLight)
											  {
												  Tooltip = VSIXResource.HasPXDBIdentityAttributeExtraInfoTooltip
											  };
			}

			switch (EffectiveBoundType)
			{		
				case BoundType.Unbound:
					yield return new TextViewModel(this, "Unbound");
					break;
				case BoundType.DbBound:
					yield return new TextViewModel(this, "Bound");
					break;
			}

			if (PropertyInfo.IsAutoNumbering)
			{
				yield return new TextViewModel(this, "Auto")
				{
					Tooltip = VSIXResource.PropertyHasAutoNumberingExtraInfoTooltip
				};
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo IElementWithTooltip.CalculateTooltip()
		{
			var attributeStrings = Children.OfType<AttributeNodeViewModel>()
										   .Select(attribute => attribute.CalculateTooltip().Tooltip);
			string aggregatedTooltip = string.Join(Environment.NewLine, attributeStrings);
			return aggregatedTooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(aggregatedTooltip);
		}
	}
}
