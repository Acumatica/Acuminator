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

			return icon switch
			{
				Icon.DacKeyProperty                             => VSIXResource.CodeMap_ExtraInfo_DacKeyIconTooltip,
				Icon.Settings                                   => VSIXResource.CodeMap_ExtraInfo_PXSetupViewIconTooltip,
				Icon.Filter                                     => VSIXResource.CodeMap_ExtraInfo_PXFilterViewIconTooltip,
				Icon.Processing when node is ViewNodeViewModel  => VSIXResource.CodeMap_ExtraInfo_ProcessingViewIconTooltip,
				Icon.Processing when node is GraphNodeViewModel => VSIXResource.CodeMap_ExtraInfo_ProcessingGraphIconTooltip,
				_                                               => Binding.DoNothing
			};
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

		private Icon GetIcon(object viewModel) =>
			viewModel switch
			{
				TreeNodeViewModel treeNode => treeNode.NodeIcon,
				IconViewModel iconViewModel => iconViewModel.IconType,
				_ => Icon.None,
			};

		private TreeNodeViewModel GetNode(object viewModel) =>
			viewModel is IconViewModel iconViewModel
				? iconViewModel.Node
				: viewModel as TreeNodeViewModel;
	}
}
