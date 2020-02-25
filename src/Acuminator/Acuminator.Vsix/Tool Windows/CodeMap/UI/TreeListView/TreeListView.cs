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
	public class TreeListView : TreeView
	{
		private GridViewColumnCollection _columns;

		public GridViewColumnCollection Columns
		{
			get
			{
				if (_columns == null)
				{
					_columns = new GridViewColumnCollection();
				}

				return _columns;
			}
		}

		protected override DependencyObject GetContainerForItemOverride() => new TreeListViewItem();

		protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeListViewItem;
	}
}
