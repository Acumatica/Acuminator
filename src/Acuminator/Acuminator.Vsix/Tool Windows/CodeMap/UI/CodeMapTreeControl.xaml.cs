using Acuminator.Vsix.ToolWindows.CodeMap;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Acuminator.Vsix.Utilities;
using System.Windows.Controls.Primitives;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interaction logic for CodeMapTreeControl.xaml
	/// </summary>
	public partial class CodeMapTreeControl : UserControl
	{
		public CodeMapTreeControl()
		{
			InitializeComponent();
		}

		private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Handled || e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
				return;

			e.Handled = true;

			if (!(sender is StackPanel treeViewItemPanel) || !(treeViewItemPanel.DataContext is TreeNodeViewModel treeNodeVM))
				return;

			treeNodeVM.NavigateToItemAsync()
					  .FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(TreeViewItem_PreviewMouseLeftButtonDown)}");
		}

		private void TreeNode_MouseEnterOrLeave(object sender, MouseEventArgs e)
		{
			if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is TreeNodeViewModel treeNode))
				return;
			
			treeNode.IsMouseOver = frameworkElement.IsMouseOver;
			e.Handled = true;	
		}
	}
}
