using System;
using System.Collections.Generic;
using System.Linq;
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
			Icon icon = IsKey
				? Icon.DacKeyProperty
				: Icon.DacProperty;
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(new IconViewModel(icon));
		}	

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																	     CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}
