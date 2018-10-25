using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;



namespace Acuminator.Vsix.ToolWindows.AntiPlagiator
{
	/// <summary>
	/// Interaction logic for AntiPlagiatorWindowControl.
	/// </summary>
	public partial class AntiPlagiatorWindowControl : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AntiPlagiatorWindowControl"/> class.
		/// </summary>
		public AntiPlagiatorWindowControl()
		{
			this.InitializeComponent();
		}

		private void DataGridCell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!(sender is DataGridCell cell) || !(cell.DataContext is PlagiarismInfoViewModel plagiarismInfo) ||
				!(cell.Tag is LocationType locationType) || e.Handled || e.ChangedButton != System.Windows.Input.MouseButton.Left)
			{
				return;
			}

			plagiarismInfo.OpenLocation(locationType);
		}
	}
}