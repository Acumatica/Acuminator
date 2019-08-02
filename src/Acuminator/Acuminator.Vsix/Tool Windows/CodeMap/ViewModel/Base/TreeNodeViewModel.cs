using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class TreeNodeViewModel : ViewModelBase
	{
		public TreeViewModel Tree { get; }

		public abstract string Name { get; protected set; }

		public ExtendedObservableCollection<TreeNodeViewModel> Children { get; } = new ExtendedObservableCollection<TreeNodeViewModel>();

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
				if (value)
				{
					Tree.SetSelectedWithoutNotification(this);
				}
				else if (ReferenceEquals(this, Tree.SelectedItem))
				{
					Tree.SetSelectedWithoutNotification(null);
				}

				NotifyPropertyChanged();
			}
		}

		public TreeNodeViewModel(TreeViewModel tree, bool isExpanded = true)
		{
			tree.ThrowOnNull(nameof(tree));

			Tree = tree;
			_isExpanded = isExpanded;
		}

		public virtual Task NavigateToItemAsync() => Microsoft.VisualStudio.Threading.TplExtensions.CompletedTask;

		public virtual void OrderChildren<TValue>(Func<TreeNodeViewModel, TValue> selector, SortDirection direction = SortDirection.Ascending)
		{
			selector.ThrowOnNull(nameof(selector));

			if (Children.Count == 0)
				return;

			var orderedChildren = 
				(direction == SortDirection.Ascending
					? Children.OrderBy(selector)
					: Children.OrderByDescending(selector))
				.ToList(Children.Count);                        //Need a copy of the collection because we can't reset collection with IEnumerable which is based on the collection

			Children.Reset(orderedChildren);
		}
	}
}
