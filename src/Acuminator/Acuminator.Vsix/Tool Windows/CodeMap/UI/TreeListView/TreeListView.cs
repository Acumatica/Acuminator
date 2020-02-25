using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap.TreeListViewControl
{
	public class TreeList : ListView
	{
        /// 
        /// Internal collection of rows representing visible nodes, actually displayed
        /// in the ListView
        /// 
        internal ExtendedObservableCollection<TreeNode> Rows
        {
            get;
            private set;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListItem;
        }
    }
}
