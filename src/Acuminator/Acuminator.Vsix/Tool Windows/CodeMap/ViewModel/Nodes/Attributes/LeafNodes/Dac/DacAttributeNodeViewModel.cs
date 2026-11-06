#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacAttributeNodeViewModel : AttributeNodeViewModel<DacAttributeInfo>
	{
		public override Icon NodeIcon => AttributeInfo.IsPXProjection
			? Icon.ProjectionAttribute
			: AttributeInfo.IsPXAccumulatorAttribute
				? Icon.PXAccumulatorAttribute
				: Icon.Attribute;

		public override bool IconDependsOnCurrentTheme => !AttributeInfo.IsPXProjection && !AttributeInfo.IsPXAccumulatorAttribute;

		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public DacAttributeNodeViewModel(TreeNodeViewModel parent, DacAttributeInfo attributeInfo, ExpandCreatedNodesFunction isExpandedCalculator) :
									base(parent, attributeInfo, isExpandedCalculator)
		{
			var dacFriendlyNameInfo = GetDacFriendlyNameForPXCacheNameAttribute();

			if (dacFriendlyNameInfo != null)
			{
				ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(dacFriendlyNameInfo);
			}
		}

		private ExtraInfoViewModel? GetDacFriendlyNameForPXCacheNameAttribute()
		{
			if (AttributeInfo.IsPXCacheName)
			{
				string? dacFriendlyName = AttributeInfo.AttributeData.GetNameFromPXCacheNameAttribute().NullIfWhiteSpace();

				if (dacFriendlyName != null)
				{
					dacFriendlyName = $"\"{dacFriendlyName}\"";
					Color color = Color.FromRgb(38, 155, 199);
					return new TextViewModel(this, dacFriendlyName, darkThemeForeground: color, lightThemeForeground: color);
				}
			}

			return null;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
