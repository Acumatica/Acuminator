using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class TreeNodeViewModel : ViewModelBase
	{
		public TreeViewModel Tree { get; }

		public string Name { get; }

		public ObservableCollection<TreeNodeViewModel> Children { get; } = new ObservableCollection<TreeNodeViewModel>();

		private bool _isExpanded;

		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool IsSelected
		{
			get => ReferenceEquals(this, Tree.SelectedItem);
			set
			{
				if (IsSelected == value)
					return;
				else if (value)
				{
					Tree.SelectedItem = this;
				}
				else if (ReferenceEquals(this, Tree.SelectedItem))
				{
					Tree.SelectedItem = null;
				}

				NotifyPropertyChanged();
			}
		}

		public TreeNodeViewModel(TreeViewModel tree, string name, bool isExpanded = true)
		{
			tree.ThrowOnNull(nameof(tree));
			name.ThrowOnNullOrWhiteSpace();

			Tree = tree;
			Name = name;
			_isExpanded = isExpanded;
		}
	}
}
