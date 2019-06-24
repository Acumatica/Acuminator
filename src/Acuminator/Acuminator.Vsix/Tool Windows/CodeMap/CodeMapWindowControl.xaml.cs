using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;



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
			if (e.Handled || e.ChangedButton != System.Windows.Input.MouseButton.Left || e.ClickCount != 2)
				return;

			e.Handled = true;

			if (!(sender is StackPanel treeViewItemPanel) || !(treeViewItemPanel.DataContext is TreeNodeViewModel treeNodeVM))
				return;

			treeNodeVM.NavigateToItemAsync()
					  .FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(TreeViewItem_PreviewMouseLeftButtonDown)}");	
		}
	}
}