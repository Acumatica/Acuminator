using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter, which detects if the tree view item is last.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeViewItem), targetType: typeof(bool))]
    class TreeViewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TreeViewItem item))
                return Binding.DoNothing;

            ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(item);
            return itemsControl.ItemContainerGenerator.IndexFromContainer(item) == itemsControl.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
             throw new NotSupportedException();
    }
}
