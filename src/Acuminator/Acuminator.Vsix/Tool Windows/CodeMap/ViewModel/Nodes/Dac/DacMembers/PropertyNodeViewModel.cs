using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using System.Threading;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PropertyNodeViewModel : DacMemberNodeViewModel
	{
		public override Icon NodeIcon => IsKey
				? Icon.DacKeyProperty
				: Icon.DacProperty;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public override string Tooltip
		{
			get
			{
				var attributeStrings = Children.OfType<AttributeNodeViewModel>().Select(attribute => attribute.Tooltip);
				return string.Join(Environment.NewLine, attributeStrings);
			}
		}

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
			var typeColor = Tree.CodeMapViewModel.IsDarkTheme
					? Color.FromRgb(86, 156, 214)
					: Color.FromRgb(160, 240, 120);

			yield return new TextViewModel(PropertyInfo.EffectivePropertyType.GetSimplifiedName(), foreground: typeColor);

			if (IsIdentity)
			{
				var idColor = Tree.CodeMapViewModel.IsDarkTheme
					? Coloriser.VSColors.DacFieldFormatColorDark
					: Coloriser.VSColors.DacFieldFormatColorLight;

				yield return new TextViewModel("ID", foreground: idColor);
			}

			switch (EffectiveBoundType)
			{		
				case BoundType.Unbound:
					yield return new TextViewModel("Unbound");
					break;
				case BoundType.DbBound:
					yield return new TextViewModel("Bound");
					break;
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
