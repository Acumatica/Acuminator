using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Acuminator.Vsix.ToolWindows.CodeMap.TreeListViewControl
{
	public class TreeListViewItem : TreeViewItem
	{
		private int? _level = null;

		public int Level
		{
			get
			{
				if (_level == null)
				{
					TreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;
					_level = parent != null 
						? (parent.Level + 1)
						: 0;
				}

				return _level.Value;
			}
		}

		protected override DependencyObject GetContainerForItemOverride() => new TreeListViewItem();
		
		protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeListViewItem;
	}
}
