#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.VisualStudio.Shell;

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

		private void TreeNode_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Handled || e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
				return;

			e.Handled = true;

			if (sender is not FrameworkElement treeNodeContainer || treeNodeContainer.DataContext is not TreeNodeViewModel treeNodeVM)
				return;

			treeNodeVM.NavigateToItemAsync()
					  .FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(TreeNode_PreviewMouseLeftButtonDown)}");
		}

		private void TreeNode_MouseEnterOrLeave(object sender, MouseEventArgs e)
		{
			if (sender is not FrameworkElement frameworkElement || frameworkElement.DataContext is not TreeNodeViewModel treeNode)
				return;
			
			treeNode.IsMouseOver = frameworkElement.IsMouseOver;
			e.Handled = true;	
		}

		private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Handled || e.ChangedButton != MouseButton.Right || e.ClickCount > 1 ||
				sender is not FrameworkElement treeNodeContainer || treeNodeContainer.DataContext is not TreeNodeViewModel treeNodeViewModel)
			{
				return;
			}

			treeNodeViewModel.IsSelected = true;
		}
	}
}
