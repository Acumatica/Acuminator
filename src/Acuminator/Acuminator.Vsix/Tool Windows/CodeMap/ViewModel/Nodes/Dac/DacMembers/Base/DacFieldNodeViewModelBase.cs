#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacFieldNodeViewModelBase : TreeNodeViewModel, 
													  IElementWithTooltip, IGroupNodeWithCyclingNavigation, IHaveDeclarationOrder
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public DacMemberCategoryNodeViewModel MemberCategory { get; }

		public DacMemberCategory MemberType => MemberCategory.CategoryType;

		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public DacFieldInfo FieldInfo { get; }

		public bool IsKey => FieldInfo.IsKey;

		public bool IsIdentity => FieldInfo.IsIdentity;

		public DacFieldCategory FieldCategory => FieldInfo.FieldCategory;

		public DbBoundnessType EffectiveDbBoundness => FieldInfo.EffectiveDbBoundness;

		public override string Name 
		{
			get => FieldInfo.Name;
			protected set { }
		}

		public int DeclarationOrder => FieldInfo.DeclarationOrder;

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => true;
		
		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex { get; set; }

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.DisplayedChildren => DisplayedChildren;

		protected DacFieldNodeViewModelBase(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent,
											DacFieldInfo fieldInfo, ExpandCreatedNodesFunction isExpandedCalculator) :
										base(dacMemberCategoryVM?.Tree!, parent, isExpandedCalculator)
		{
			MemberCategory = dacMemberCategoryVM!;
			FieldInfo = fieldInfo.CheckIfNull();

			var extraInfos = GetExtraInfos().ToList(capacity: 4);

			if (extraInfos.Count > 0)
			{
				ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(extraInfos);
			}
		}

		private IEnumerable<ExtraInfoViewModel> GetExtraInfos()
		{
			if (FieldInfo.PropertyTypeUnwrappedNullable != null)
			{
				yield return new TextViewModel(this, FieldInfo.PropertyTypeUnwrappedNullable.GetSimplifiedName(),
												darkThemeForeground: Color.FromRgb(86, 156, 214),
												lightThemeForeground: Color.FromRgb(0, 0, 255));
			}

			if (IsIdentity)
			{
				yield return new TextViewModel(this, "ID",
												darkThemeForeground: Coloriser.VSColors.DacFieldFormatColorDark,
												lightThemeForeground: Coloriser.VSColors.DacFieldFormatColorLight)
								 {
									 Tooltip = VSIXResource.CodeMap_ExtraInfo_HasPXDBIdentityAttributeTooltip
								 };
			}

			string? boundLabelText = GetDbBoundnessLabelText();

			if (boundLabelText != null)
				yield return new TextViewModel(this, boundLabelText);

			if (FieldInfo.IsAutoNumbering)
			{
				yield return new TextViewModel(this, "Auto")
				{
					Tooltip = VSIXResource.CodeMap_ExtraInfo_PropertyHasAutoNumberingTooltip
				};
			}
		}

		private string? GetDbBoundnessLabelText() => EffectiveDbBoundness switch
		{
			DbBoundnessType.Unbound    => VSIXResource.CodeMap_DbBoundnessIndicator_Unbound,
			DbBoundnessType.DbBound	   => VSIXResource.CodeMap_DbBoundnessIndicator_Bound,
			DbBoundnessType.PXDBCalced => VSIXResource.CodeMap_DbBoundnessIndicator_PXDBCalced,
			DbBoundnessType.PXDBScalar => VSIXResource.CodeMap_DbBoundnessIndicator_PXDBScalar,
			DbBoundnessType.Unknown    => VSIXResource.CodeMap_DbBoundnessIndicator_Unknown,
			DbBoundnessType.Error 	   => VSIXResource.CodeMap_DbBoundnessIndicator_Inconsistent,
			_ 						   => null
		};

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var propertyNode = AllChildren.OfType<DacFieldPropertyNodeViewModel>().FirstOrDefault();
			return propertyNode is IElementWithTooltip elementWithTooltip
				? elementWithTooltip.CalculateTooltip()
				: null;
		}

		public async override Task NavigateToItemAsync()
		{
			var childToNavigateTo = this.GetChildToNavigateTo();

			if (childToNavigateTo != null)
			{
				await childToNavigateTo.NavigateToItemAsync();
				IsExpanded = true;
				Tree.SelectedItem = childToNavigateTo;
			}
		}

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => true;
	}
}
