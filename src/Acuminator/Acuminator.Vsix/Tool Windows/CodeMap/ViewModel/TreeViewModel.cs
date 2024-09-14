#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TreeViewModel : ViewModelBase
	{
		public CodeMapWindowViewModel CodeMapViewModel { get; }

		public ExtendedObservableCollection<TreeNodeViewModel> AllRootItems { get; } = new();

		private readonly ExtendedObservableCollection<TreeNodeViewModel> _mutableDisplayedRoots = new();

		public ReadOnlyObservableCollection<TreeNodeViewModel> DisplayedRootItems { get; }

		public ExtendedObservableCollection<TreeNodeViewModel> AllItems { get; } = new();

		private TreeNodeViewModel? _selectedItem;

		public TreeNodeViewModel? SelectedItem
		{
			get => _selectedItem;
			set
			{
				TreeNodeViewModel? previousSelection = _selectedItem;
				_selectedItem = value;

				if (previousSelection != null)
					previousSelection.IsSelected = false;

				if (_selectedItem != null)
					_selectedItem.IsSelected = true;

				NotifyPropertyChanged();
			}
		}

		private bool _isExtraInfoVisible = true;

		public bool IsExtraInfoVisible
		{
			get => _isExtraInfoVisible;
			set
			{
				if (_isExtraInfoVisible != value)
				{
					_isExtraInfoVisible = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// A workaround to avoid endless loop of TreeNodeViewModel IsSelected and TreeViewModel SelectedItem setting each other.
		/// </summary>
		/// <param name="selected">The selected.</param>
		internal void SetSelectedWithoutNotification(TreeNodeViewModel? selected)
		{
			_selectedItem = selected;
			NotifyPropertyChanged(nameof(SelectedItem));
		}

		public TreeViewModel(CodeMapWindowViewModel windowViewModel)
		{
			CodeMapViewModel = windowViewModel.CheckIfNull();

			AllRootItems.CollectionChanged += AllRootItems_CollectionChanged;
			DisplayedRootItems = new ReadOnlyObservableCollection<TreeNodeViewModel>(_mutableDisplayedRoots);
		}

		public void Clear()
		{
			AllRootItems.Clear();
			AllItems.Clear();
		}

		public void FillCodeMapTree(IEnumerable<TreeNodeViewModel> roots)
		{
			if (roots.IsNullOrEmpty())
			{
				Clear();
				return;
			}

			AllRootItems.Reset(roots);

			var flattenedTree = AllRootItems.SelectMany(root => root.AllDescendantsAndSelf());
			AllItems.Reset(flattenedTree);
		}

		public void RefreshFlattenedNodesList()
		{
			var flattenedTree = AllRootItems.SelectMany(root => root.AllDescendantsAndSelf());
			AllItems.Reset(flattenedTree);
		}

		public void SubscribeOnDisplayedRootsCollectionChanged(NotifyCollectionChangedEventHandler collectionChangedEventHandler)
		{
			if (collectionChangedEventHandler != null)
				_mutableDisplayedRoots.CollectionChanged += collectionChangedEventHandler;
		}

		private void AllRootItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var visibleChildren = e.Action != NotifyCollectionChangedAction.Move
				? AllRootItems.Where(child => child.IsVisible)
				: AllRootItems;

			_mutableDisplayedRoots.Reset(visibleChildren);
		}
	}
}
