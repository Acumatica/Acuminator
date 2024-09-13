#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Vsix.ToolWindows.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class AttributesGroupNodeViewModel : TreeNodeViewModel, IGroupNodeWithCyclingNavigation, IElementWithTooltip
	{
		public override Icon NodeIcon => Icon.AttributesGroup;

		public abstract AttributePlacement Placement { get; }

		protected abstract string AttributesGroupDescription { get; }

		public override string Name
		{
			get => $"{AttributesGroupDescription}({DisplayedChildren.Count})";
			protected set { }
		}

		protected abstract bool AllowNavigation { get; }

		bool IGroupNodeWithCyclingNavigation.AllowNavigation => AllowNavigation;

		int IGroupNodeWithCyclingNavigation.CurrentNavigationIndex
		{
			get;
			set;
		}

		IList<TreeNodeViewModel> IGroupNodeWithCyclingNavigation.DisplayedChildren => DisplayedChildren;

		protected AttributesGroupNodeViewModel(TreeNodeViewModel parent, bool isExpanded = false) :
										  base(parent?.Tree!, parent, isExpanded)
		{
			
		}

		public abstract IEnumerable<AttributeInfoBase> UntypedAttributeInfos();

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

		bool IGroupNodeWithCyclingNavigation.CanNavigateToChild(TreeNodeViewModel child) => CanNavigateToChild(child);

		protected virtual bool CanNavigateToChild(TreeNodeViewModel child) => true;

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var attributeStrings = AllChildren.OfType<AttributeNodeViewModel>()
											  .Select(attribute => attribute.CalculateTooltip().Tooltip);
			string aggregatedTooltip = attributeStrings.Join(Environment.NewLine);
			return aggregatedTooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(aggregatedTooltip);
		}
	}
}
