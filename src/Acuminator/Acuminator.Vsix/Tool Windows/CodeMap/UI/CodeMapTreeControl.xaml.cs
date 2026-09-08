#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.VisualStudio.Shell;

using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interaction logic for CodeMapTreeControl.xaml
	/// </summary>
	public partial class CodeMapTreeControl : UserControl
	{
		private bool _isSelectionChanging;
		private bool _isFilterSelectionUpdating;

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
			
			treeNodeVM.Tree.SelectedItem = treeNodeVM;

			if (e.ClickCount >= 2)
			{
				var cancellation = treeNodeVM.Tree.CodeMapViewModel.CancellationToken ?? AcuminatorVSPackage.Instance?.DisposalToken ?? default;
				NavigateOnClickAsync(treeNodeVM)
					.FileAndForgetAcuminatorTask($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(TreeNode_PreviewMouseLeftButtonDown)}");
			}
		}

		private async System.Threading.Tasks.Task NavigateOnClickAsync(TreeNodeViewModel treeNodeVM)
		{
			Cursor oldCursor = Mouse.OverrideCursor;

			try
			{
				Mouse.OverrideCursor = Cursors.Wait;

				await treeNodeVM.NavigateToItemAsync();
			}
			finally
			{
				Mouse.OverrideCursor = oldCursor;
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

			treeNodeViewModel.Tree.SelectedItem = treeNodeViewModel;
		}

		private void NameTextBox_Initialized(object sender, EventArgs e)
		{
			if (sender is not TextBox textBox || textBox.DataContext is not TreeNodeViewModel treeNodeVM)
				return;

			try
			{
				_isFilterSelectionUpdating = true;

				UpdateFilterSelection(textBox, treeNodeVM);
			}
			finally
			{
				_isFilterSelectionUpdating = false;
			}
		}

		private void NameTextBox_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (_isSelectionChanging || _isFilterSelectionUpdating || sender is not TextBox textBox)
				return;

			try
			{
				_isSelectionChanging = true;

				if (textBox.DataContext is TreeNodeViewModel treeNodeVM)
					UpdateFilterSelection(textBox, treeNodeVM);
				else
					textBox.SelectionLength = 0;

				e.Handled = true;
			}
			finally
			{
				_isSelectionChanging = false;
			}
		}

		private void UpdateFilterSelection(TextBox textBox, TreeNodeViewModel treeNodeVM)
		{
			if (!treeNodeVM.IsVisible)
			{
				textBox.SelectionLength = 0;
				return;
			}

			var filterVM = treeNodeVM.Tree.CodeMapViewModel.FilterVM;

			if (filterVM.HasFilterText)
			{
				string filterTextTrimmed = filterVM.FilterText.Trim();
				int selectionIndex = textBox.Text.IndexOf(filterTextTrimmed, StringComparison.OrdinalIgnoreCase);

				if (selectionIndex < 0)
				{
					textBox.SelectionLength = 0;
					return;
				}

				textBox.Select(selectionIndex, filterTextTrimmed.Length);
			}
			else
			{
				textBox.SelectionLength = 0;
				return;
			}
		}

		private void CodeMapIcon_DpiChanged(object sender, DpiChangedEventArgs e)
		{
			if (sender is Image iconImage &&
				(e.OldDpi.DpiScaleX != e.NewDpi.DpiScaleX || e.OldDpi.DpiScaleY != e.NewDpi.DpiScaleY))
			{
				DpiUtils.SetReverseDpiTransformationForImage(iconImage);
			}
		}

		private void CodeMapIcon_Initialized(object sender, EventArgs e) =>
			DpiUtils.SetReverseDpiTransformationForImage(sender as Image);
	}
}
