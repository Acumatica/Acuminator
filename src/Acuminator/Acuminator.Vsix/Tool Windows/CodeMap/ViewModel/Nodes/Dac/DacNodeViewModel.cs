#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacNodeViewModel : TreeNodeViewModel, IElementWithTooltip
	{
		public DacSemanticModel DacModel { get; }

		public override string Name
		{
			get => DacModel.Name;
			protected set { }
		}

		public override Icon NodeIcon => DacModel.DacType == DacType.Dac
			? Icon.Dac
			: Icon.DacExtension;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public DacNodeViewModel(DacSemanticModel dacModel, TreeViewModel tree, bool isExpanded) : 
						   base(tree, parent: null, isExpanded)
		{
			DacModel = dacModel.CheckIfNull();
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetDacExtraInfos()
		{
			if (DacModel.IsProjectionDac)
			{
				yield return new IconViewModel(this, Icon.ProjectionDac);
			}

			Color color = Color.FromRgb(38, 155, 199);

			string dacType = DacModel.DacType == DacType.Dac
				? VSIXResource.CodeMap_ExtraInfo_IsDac
				: VSIXResource.CodeMap_ExtraInfo_IsDacExtension;

			yield return new TextViewModel(this, dacType, darkThemeForeground: color, lightThemeForeground: color);

			var pxCacheNameAttributeInfo = DacModel.Attributes.FirstOrDefault(attrInfo => attrInfo.IsPXCacheName);
			string? dacFriendlyName = pxCacheNameAttributeInfo?.AttributeData.GetNameFromPXCacheNameAttribute()
																			 .NullIfWhiteSpace();
			if (dacFriendlyName != null)
			{
				dacFriendlyName = $"\"{dacFriendlyName}\"";
				yield return new TextViewModel(this, dacFriendlyName, darkThemeForeground: color, lightThemeForeground: color);
			}
		}

		public override Task NavigateToItemAsync() => DacModel.Symbol.NavigateToAsync();

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var dacAttributesGroupNode = AllChildren.OfType<DacAttributesGroupNodeViewModel>().FirstOrDefault();
			return dacAttributesGroupNode is IElementWithTooltip elementWithTooltip
				? elementWithTooltip.CalculateTooltip()
				: null;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}