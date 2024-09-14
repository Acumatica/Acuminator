#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
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

		private readonly ExtendedObservableCollection<TreeNodeViewModel> _mutableAllDisplayedItems = new();

		public ReadOnlyObservableCollection<TreeNodeViewModel> AllDisplayedItems { get; }

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
			if (ReferenceEquals(SelectedItem, selected)) 
				return;

			var oldSelectedItem = SelectedItem;
			oldSelectedItem?.NotifyPropertyChanged(nameof(oldSelectedItem.IsSelected));
			
			_selectedItem = selected;

			NotifyPropertyChanged(nameof(SelectedItem));
			_selectedItem?.NotifyPropertyChanged(nameof(_selectedItem.IsSelected));
		}

		public TreeViewModel(CodeMapWindowViewModel windowViewModel)
		{
			CodeMapViewModel = windowViewModel.CheckIfNull();

			AllRootItems.CollectionChanged += AllRootItems_CollectionChanged;
			DisplayedRootItems = new ReadOnlyObservableCollection<TreeNodeViewModel>(_mutableDisplayedRoots);

			AllItems.CollectionChanged += AllItems_CollectionChanged;
			AllDisplayedItems = new ReadOnlyObservableCollection<TreeNodeViewModel>(_mutableAllDisplayedItems);
		}

		public void Clear()
		{
			AllRootItems.Clear();
			AllItems.Clear();
		}

		public void FillCodeMapTree(IEnumerable<TreeNodeViewModel> roots, FilterOptions? filterOptions)
		{
			if (roots.IsNullOrEmpty())
			{
				Clear();
				return;
			}

			// Unsubscribe from the synchronization with displayed items temporarily for optimization
			// Since the displayed items will be updated anyway
			try
			{
				AllRootItems.CollectionChanged -= AllRootItems_CollectionChanged;
				AllItems.CollectionChanged	   -= AllItems_CollectionChanged;

				AllRootItems.Reset(roots);

				RefreshFlattenedNodesList();
			}
			finally
			{
				AllRootItems.CollectionChanged += AllRootItems_CollectionChanged;
				AllItems.CollectionChanged	   += AllItems_CollectionChanged;
			}

			RefreshNodesVisibleInFilter(filterOptions);
		}

		public void RefreshFlattenedNodesList()
		{
			var flattenedTree = AllRootItems.SelectMany(root => root.AllDescendantsAndSelfDFS());
			AllItems.Reset(flattenedTree);
		}

		public void RefreshNodesVisibleInFilter(FilterOptions? filterOptions)
		{
			foreach (var root in AllRootItems)
			{
				root.RefreshVisibilityForNodeAndSubTreeFromFilter(filterOptions);
			}

			if (filterOptions == null || !filterOptions.HasFilter)
			{
				_mutableDisplayedRoots.Reset(AllRootItems);
				_mutableAllDisplayedItems.Reset(AllItems);
			}
			else
			{
				RefreshDisplayedRoots();
				RefreshAllDisplayedItems();
			}

			if (SelectedItem != null && !SelectedItem.IsVisible)
				SelectedItem = null;
		}

		public void RefreshDisplayedRoots() => RefreshDisplayedCollection(_mutableDisplayedRoots, AllRootItems);

		public void RefreshAllDisplayedItems() => RefreshDisplayedCollection(_mutableAllDisplayedItems, AllItems);

		public void SubscribeOnDisplayedRootsCollectionChanged(NotifyCollectionChangedEventHandler collectionChangedEventHandler)
		{
			if (collectionChangedEventHandler != null)
				_mutableDisplayedRoots.CollectionChanged += collectionChangedEventHandler;
		}

		public void SubscribeOnAllDisplayedItemsCollectionChanged(NotifyCollectionChangedEventHandler collectionChangedEventHandler)
		{
			if (collectionChangedEventHandler != null)
				_mutableAllDisplayedItems.CollectionChanged += collectionChangedEventHandler;
		}

		private void AllRootItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshDisplayedRoots();

		private void AllItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshAllDisplayedItems();

		private static void RefreshDisplayedCollection(ExtendedObservableCollection<TreeNodeViewModel> displayedCollection,
													   ExtendedObservableCollection<TreeNodeViewModel> allItemsCollection)
		{
			var visibleNodes = allItemsCollection.Where(node => node.IsVisible);
			displayedCollection.Reset(visibleNodes);
		}
	}
}
