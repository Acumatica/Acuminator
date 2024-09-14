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
			if (e.Handled || e.ChangedButton != MouseButton.Left ||
				sender is not FrameworkElement treeNodeContainer || treeNodeContainer.DataContext is not TreeNodeViewModel treeNodeVM)
			{
				return;
			}
			
			treeNodeVM.IsSelected = true;

			if (e.ClickCount >= 2)
			{
				treeNodeVM.NavigateToItemAsync()
						  .FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(TreeNode_PreviewMouseLeftButtonDown)}");
			}
		}

		private void TreeNode_MouseEnterOrLeave(object sender, MouseEventArgs e)
		{
			if (sender is not FrameworkElement frameworkElement || frameworkElement.DataContext is not TreeNodeViewModel treeNode)
				return;
			
			treeNode.IsMouseOver = frameworkElement.IsMouseOver;
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

		private void NameTextBox_Initialized(object sender, EventArgs e)
		{
			if (sender is not TextBox textBox || textBox.DataContext is not TreeNodeViewModel treeNodeVM)
			{
				return;
			}

			if (!treeNodeVM.IsVisible)
			{
				textBox.SelectionLength = 0;
				return;
			}

			var filterVM = treeNodeVM.Tree.CodeMapViewModel.FilterVM;

			if (filterVM.HasFilterText)
			{
				int selectionIndex = textBox.Text.IndexOf(filterVM.FilterText, StringComparison.OrdinalIgnoreCase);

				if (selectionIndex < 0)
				{
					textBox.SelectionLength = 0;
					return;
				}

				textBox.Select(selectionIndex, filterVM.FilterText.Length);
			}
			else
			{
				textBox.SelectionLength = 0;
				return;
			}
		}
	}
}
