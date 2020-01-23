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

		/// <summary>
		/// The tooltip displayed for node.
		/// </summary>
		public virtual string Tooltip => null;

		/// <summary>
		/// The icon for a node.
		/// </summary>
		public virtual Icon NodeIcon => Icon.None;

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

		public Command ExpandOrCollapseAllCommand { get; }

		protected TreeNodeViewModel(TreeViewModel tree, bool isExpanded = true)
		{
			tree.ThrowOnNull(nameof(tree));

			Tree = tree;
			_isExpanded = isExpanded;
			ExpandOrCollapseAllCommand = new Command(p => ExpandOrCollapseAll(expand: !IsExpanded));
		}

		public virtual Task NavigateToItemAsync() => Microsoft.VisualStudio.Threading.TplExtensions.CompletedTask;

		public virtual void AcceptBuilder(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation)
		{
			treeBuilder.ThrowOnNull(nameof(treeBuilder));
			var children = CreateChildren(treeBuilder, expandChildren, cancellation)?.ToList();

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

		protected virtual void ExpandOrCollapseAll(bool expand)
		{
			IsExpanded = expand;
			Children.ForEach(childNode => childNode.ExpandOrCollapseAll(expand));
		}
	}
}
