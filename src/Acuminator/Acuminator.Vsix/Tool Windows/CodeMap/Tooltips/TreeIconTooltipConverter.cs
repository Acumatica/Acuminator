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

			switch (icon)
			{
				case Icon.DacKeyProperty:
					return VSIXResource.DacKeyIconTooltip;
				case Icon.Settings:
					return VSIXResource.PXSetupViewIconTooltip;
				case Icon.Filter:
					return VSIXResource.PXFilterViewIconTooltip;
				case Icon.Processing:
					return VSIXResource.ProcessingViewIconTooltip;
				default:
					return Binding.DoNothing;
			}
		}

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

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
