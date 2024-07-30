#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacAttributeNodeViewModel : AttributeNodeViewModel<DacAttributeInfo>
	{
		public override Icon NodeIcon => AttributeInfo.IsPXProjection
			? Icon.ProjectionAttribute
			: Icon.Attribute;

		public override bool IconDependsOnCurrentTheme => !AttributeInfo.IsPXProjection;

		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public DacAttributeNodeViewModel(TreeNodeViewModel nodeVM, DacAttributeInfo attributeInfo, bool isExpanded = false) :
									base(nodeVM, attributeInfo, isExpanded)
		{
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetDacExtraInfos()
		{
			if (AttributeInfo.IsPXCacheName)
			{
				string? dacFriendlyName = AttributeInfo.AttributeData.GetNameFromPXCacheNameAttribute().NullIfWhiteSpace();

				if (dacFriendlyName != null)
				{
					dacFriendlyName = $"\"{dacFriendlyName}\"";
					Color color = Color.FromRgb(38, 155, 199);
					yield return new TextViewModel(this, dacFriendlyName, darkThemeForeground: color, lightThemeForeground: color);
				}
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
