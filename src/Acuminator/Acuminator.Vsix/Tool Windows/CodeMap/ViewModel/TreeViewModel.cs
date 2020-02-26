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

		public ExtendedObservableCollection<TreeNodeViewModel> AllItems { get; } = new ExtendedObservableCollection<TreeNodeViewModel>();

		private TreeNodeViewModel _selectedItem;

		public TreeNodeViewModel SelectedItem
		{
			get => _selectedItem;
			set
			{
				TreeNodeViewModel previousSelection = _selectedItem;
				_selectedItem = value;

				if (previousSelection != null)
					previousSelection.IsSelected = false;

				if (_selectedItem != null)
					_selectedItem.IsSelected = true;

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// A workaround to avoid endless loop of TreeNodeViewModel IsSelected and TreeViewModel SelectedItem setting each other.
		/// </summary>
		/// <param name="selected">The selected.</param>
		internal void SetSelectedWithoutNotification(TreeNodeViewModel selected)
		{
			_selectedItem = selected;
			NotifyPropertyChanged(nameof(SelectedItem));
		}

		public TreeViewModel(CodeMapWindowViewModel windowViewModel)
		{
			windowViewModel.ThrowOnNull(nameof(windowViewModel));

			CodeMapViewModel = windowViewModel;
		}

		public void Clear()
		{
			RootItems.Clear();
			AllItems.Clear();
		}

		public void FillCodeMapTree(IEnumerable<TreeNodeViewModel> roots)
		{
			if (roots.IsNullOrEmpty())
			{
				Clear();
				return;
			}

			RootItems.Reset(roots);

			var flattenedTree = RootItems.SelectMany(root => GetNodeWithDescendants(root));
			AllItems.Reset(flattenedTree);
		}

		public void RefreshFlattenedNodesList()
		{
			var flattenedTree = RootItems.SelectMany(root => GetNodeWithDescendants(root));
			AllItems.Reset(flattenedTree);
		}

		private IEnumerable<TreeNodeViewModel> GetNodeWithDescendants(TreeNodeViewModel node)
		{
			yield return node;

			if (node.Children.Count == 0)
				yield break;

			var descendants = node.Children.SelectMany(child => GetNodeWithDescendants(child));

			foreach (var descendant in descendants)
			{
				yield return descendant;
			}
		}
	}
}
