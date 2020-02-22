using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using System.Threading;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class PropertyNodeViewModel : DacMemberNodeViewModel
	{
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

		public BoundType BoundType => PropertyInfo.BoundType;

		public PropertyNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, DacPropertyInfo propertyInfo, bool isExpanded = false) :
								base(dacMemberCategoryVM, propertyInfo, isExpanded)
		{
			var extraInfos = GetExtraInfos();
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(extraInfos);
		}	

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																	     CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);

		private IEnumerable<ExtraInfoViewModel> GetExtraInfos()
		{
			Icon icon = IsKey
				? Icon.DacKeyProperty
				: Icon.DacProperty;

			yield return new IconViewModel(icon);

			if (IsIdentity)
			{
				yield return new TextViewModel("ID", Brushes.LightGreen);
			}

			switch (BoundType)
			{		
				case BoundType.Unbound:
					yield return new TextViewModel("Unbound", Brushes.SaddleBrown);
					break;
				case BoundType.DbBound:
					yield return new TextViewModel("Bound", Brushes.SaddleBrown);
					break;
			}
		}
		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
