using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class TreeNodeViewModel : ViewModelBase
	{
		public TreeViewModel Tree { get; }

		public abstract string Name { get; protected set; }

		/// <summary>
		/// True to display node without children in a tree, false if not.
		/// </summary>
		public abstract bool DisplayNodeWithoutChildren { get; }

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

		protected TreeNodeViewModel(TreeViewModel tree, bool isExpanded = true)
		{
			tree.ThrowOnNull(nameof(tree));

			Tree = tree;
			_isExpanded = isExpanded;
		}

		public virtual Task NavigateToItemAsync() => Microsoft.VisualStudio.Threading.TplExtensions.CompletedTask;

		public virtual void AcceptBuilder(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation)
		{
			treeBuilder.ThrowOnNull(nameof(treeBuilder));
			var children = CreateChildren(treeBuilder, expandChildren, cancellation);

			if (children.IsNullOrEmpty())
				return;

			foreach (var child in children)
			{
				child?.AcceptBuilder(treeBuilder, expandChildren, cancellation);
			}

			var childrenToSet = children.Where(c => c != null && (c.Children.Count > 0 || c.DisplayNodeWithoutChildren));
			Children.Reset(childrenToSet);
		}

		protected abstract IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren,
																	     CancellationToken cancellation);
	}
}
