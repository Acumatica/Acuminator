using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the icon tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeIconTooltipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Icon icon = GetIcon(value);
			var node = GetNode(value);

			switch (icon)
			{
				case Icon.DacKeyProperty:
					return VSIXResource.DacKeyIconTooltip;
				case Icon.Settings:
					return VSIXResource.PXSetupViewIconTooltip;
				case Icon.Filter:
					return VSIXResource.PXFilterViewIconTooltip;
				case Icon.Processing when node is ViewNodeViewModel:
					return VSIXResource.ProcessingViewIconTooltip;
				case Icon.Processing when node is GraphNodeViewModel:
					return VSIXResource.ProcessingGraphIconTooltip;
				default:
					return Binding.DoNothing;
			}
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

		private Icon GetIcon(object viewModel)
		{
			switch (viewModel)
			{
				case TreeNodeViewModel treeNode:
					return treeNode.NodeIcon;

				case IconViewModel iconViewModel:
					return iconViewModel.IconType;

				default:
					return Icon.None;
			}
		}

		private TreeNodeViewModel GetNode(object viewModel) =>
			viewModel is IconViewModel iconViewModel
				? iconViewModel.Node
				: viewModel as TreeNodeViewModel;
	}
}
