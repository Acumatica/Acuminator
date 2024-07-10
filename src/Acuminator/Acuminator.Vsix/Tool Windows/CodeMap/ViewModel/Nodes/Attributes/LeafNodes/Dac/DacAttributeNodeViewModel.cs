#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacAttributeNodeViewModel : AttributeNodeViewModel<DacAttributeInfo>
	{
		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public DacAttributeNodeViewModel(TreeNodeViewModel nodeVM, DacAttributeInfo attributeInfo, bool isExpanded = false) :
									base(nodeVM, attributeInfo, isExpanded)
		{
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetDacExtraInfos()
		{
			if (AttributeInfo.IsPXProjection)
			{
				yield return new IconViewModel(this, Icon.ProjectionAttribute);
			}

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
