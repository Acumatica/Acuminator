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
				Icon.DacKeyProperty                             => VSIXResource.DacKeyIconTooltip,
				Icon.Settings                                   => VSIXResource.PXSetupViewIconTooltip,
				Icon.Filter                                     => VSIXResource.PXFilterViewIconTooltip,
				Icon.Processing when node is ViewNodeViewModel  => VSIXResource.ProcessingViewIconTooltip,
				Icon.Processing when node is GraphNodeViewModel => VSIXResource.ProcessingGraphIconTooltip,
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
