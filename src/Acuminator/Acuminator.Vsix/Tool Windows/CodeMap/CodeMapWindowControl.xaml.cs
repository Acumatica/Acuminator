using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interaction logic for CodeMapWindowControl.
	/// </summary>
	public partial class CodeMapWindowControl : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CodeMapWindowControl"/> class.
		/// </summary>
		public CodeMapWindowControl()
		{
			this.InitializeComponent();
		}

		private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.Handled || e.ClickCount != 2 || e.ChangedButton != System.Windows.Input.MouseButton.Left)
				return;

			if (!(sender is StackPanel treeViewItemPanel) || !(treeViewItemPanel.DataContext is TreeNodeViewModel treeNodeVM))
				return;

			treeNodeVM.NavigateToItem();
			e.Handled = true;
		}
	}
}