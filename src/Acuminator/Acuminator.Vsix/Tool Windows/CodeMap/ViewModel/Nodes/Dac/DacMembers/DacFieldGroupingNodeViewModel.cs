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
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeViewModel : TreeNodeViewModel, IElementWithTooltip, IGroupNodeWithCyclingNavigation
	{
		public DacMemberCategoryNodeViewModel MemberCategory { get; }

		public DacMemberCategory MemberType => MemberCategory.CategoryType;

		public override Icon NodeIcon => IsKey
				? Icon.DacKeyField
				: Icon.DacField;

		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public DacFieldInfo FieldInfo { get; }

		public bool IsDacProperty => FieldInfo.IsDacProperty;

		public bool IsKey => FieldInfo.IsKey;

		public bool IsIdentity => FieldInfo.IsIdentity;

		public DbBoundnessType EffectiveDbBoundness => FieldInfo.EffectiveDbBoundness;

		public override string Name 
		{
			get => FieldInfo.Name;
			protected set { }
		}

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => true;
		
		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex { get; set; }

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.Children => Children;

		public override bool DisplayNodeWithoutChildren => false;

		public DacFieldGroupingNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent,
											 DacFieldInfo fieldInfo, bool isExpanded = false) :
										base(dacMemberCategoryVM?.Tree!, parent, isExpanded)
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
			if (FieldInfo.EffectivePropertyType != null)
			{
				yield return new TextViewModel(this, FieldInfo.EffectivePropertyType.GetSimplifiedName(),
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

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var propertyNode = Children.OfType<DacFieldPropertyNodeViewModel>().FirstOrDefault();
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
