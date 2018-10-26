using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using ForceGraph;


using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.DacExplorer
{
	public class DacExplorerViewModel : ToolWindowViewModelBase
	{
		public ExtendedObservableCollection<DacViewModel> DACs { get; } = new ExtendedObservableCollection<DacViewModel>();

		private DacViewModel _selectedItem;

		public DacViewModel SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (!ReferenceEquals(_selectedItem, value))
				{
					_selectedItem = value;
					NotifyPropertyChanged();

					ForceGraphViewModel.ForceGraphScene?.CreateDacVertices(null, SelectedItem.DacName);
				}
			}
		}

		public Command AllDACsCommand { get; }

		public ForceGraphViewModel ForceGraphViewModel { get; }

		public DacExplorerViewModel()
		{
			ForceGraphViewModel = new ForceGraphViewModel();
			ForceGraphViewModel.OnForceGraphUpdated += ForceGraphViewModel_OnForceGraphUpdated;
			AllDACsCommand = new Command(p => CreateAllDACs());
		}

		private void ForceGraphViewModel_OnForceGraphUpdated(object sender, EventArgs e)
		{
			DACs.Clear();

			var dacs =ForceGraphViewModel.ForceGraph.GetDacs().Select(dacName => new DacViewModel(this, dacName));
			DACs.AddRange(dacs);
		}

		private void CreateAllDACs()
		{
			ForceGraphViewModel.ForceGraphScene?.CreateVertices(null);
		}
	}
}
