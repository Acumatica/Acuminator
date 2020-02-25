using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TreeListViewItem : TreeViewItem
	{
		protected override DependencyObject GetContainerForItemOverride() => new TreeListViewItem();
		
		protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeListViewItem;
	}
}
