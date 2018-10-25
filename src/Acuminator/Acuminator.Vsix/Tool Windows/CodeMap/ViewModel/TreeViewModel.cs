using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TreeViewModel : ViewModelBase
	{
		public CodeMapWindowViewModel CodeMapViewModel { get; } 

		public ExtendedObservableCollection<TreeNodeViewModel> RootItems { get; } = new ExtendedObservableCollection<TreeNodeViewModel>();

		private TreeNodeViewModel _selectedItem;

		public TreeNodeViewModel SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (ReferenceEquals(_selectedItem, value))
					return;

				TreeNodeViewModel previousSelection = _selectedItem;
				_selectedItem = value;

				if (previousSelection != null)
					previousSelection.IsSelected = false;

				if (_selectedItem != null)
					_selectedItem.IsSelected = true;

				NotifyPropertyChanged();
			}
		}

		public TreeViewModel(CodeMapWindowViewModel windowViewModel)
		{
			windowViewModel.ThrowOnNull(nameof(windowViewModel));

			CodeMapViewModel = windowViewModel;
		}
	}
}
