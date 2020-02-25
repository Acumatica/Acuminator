using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.TreeListViewControl
{
	public sealed partial class TreeNode : ViewModelBase
	{
		internal TreeList Tree { get; }

		private INotifyCollectionChanged _childrenSource;

		internal INotifyCollectionChanged ChildrenSource
		{
			get { return _childrenSource; }
			set
			{
				if (_childrenSource != null)
				{
					_childrenSource.CollectionChanged -= ChildrenChanged;
				}

				_childrenSource = value;

				if (_childrenSource != null)
				{
					_childrenSource.CollectionChanged += ChildrenChanged;
				}
			}
		}

		public int Index
		{
			get;
			private set;
		}

		/// <summary>
		/// Returns true if all parent nodes of this node are expanded.
		/// </summary>
		internal bool IsVisible
		{
			get
			{
				TreeNode node = Parent;

				while (node != null)
				{
					if (!node.IsExpanded)
						return false;

					node = node.Parent;
				}

				return true;
			}
		}

		public bool IsExpandedOnce
		{
			get;
			internal set;
		}

		public bool HasChildren
		{
			get;
			internal set;
		}

		private bool _isExpanded;

		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (value != IsExpanded)
				{
					Tree.SetIsExpanded(this, value);
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(IsExpandable));
				}
			}
		}

		internal void AssignIsExpanded(bool value)
		{
			_isExpanded = value;
		}

		public bool IsExpandable => (HasChildren && !IsExpandedOnce) || Nodes.Count > 0;

		private bool _isSelected;

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (value != _isSelected)
				{
					_isSelected = value;
					NotifyPropertyChanged();
				}
			}
		}

		public TreeNode Parent
		{
			get;
			private set;
		}

		public int Level => Parent == null
			? -1
			: Parent.Level + 1;

		public TreeNode PreviousNode => Parent != null && Index > 0
			? Parent.Nodes[Index - 1]
			: null;

		public TreeNode NextNode => Parent != null && Index < Parent.Nodes.Count - 1
			? Parent.Nodes[Index + 1]
			: null;

		internal TreeNode BottomNode =>
			Parent == null
				? null
				: (Parent.NextNode ?? Parent.BottomNode);

		internal TreeNode NextVisibleNode =>
			IsExpanded && Nodes.Count > 0
				? Nodes[0]
				: NextNode ?? BottomNode;

		public int VisibleChildrenCount => AllVisibleChildren.Count();

		public IEnumerable<TreeNode> AllVisibleChildren
		{
			get
			{
				int level = this.Level;
				TreeNode node = this;
				while (true)
				{
					node = node.NextVisibleNode;
					if (node != null && node.Level > level)
						yield return node;
					else
						break;
				}
			}
		}

		public object Tag { get; }

		internal Collection<TreeNode> Children { get; }

		public ReadOnlyCollection<TreeNode> Nodes { get; }

		internal TreeNode(TreeList tree, object tag)
		{
			Tree = tree.CheckIfNull(nameof(tree));
			Children = new NodeCollection(this);
			Nodes = new ReadOnlyCollection<TreeNode>(Children);
			Tag = tag;
		}

		public override string ToString() => Tag?.ToString() ?? base.ToString();

		void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						int index = e.NewStartingIndex;
						int rowIndex = Tree.Rows.IndexOf(this);

						foreach (object obj in e.NewItems)
						{
							Tree.InsertNewNode(this, obj, rowIndex, index);
							index++;
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					if (Children.Count > e.OldStartingIndex)
					{
						RemoveChildAt(e.OldStartingIndex);
					}

					break;

				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					while (Children.Count > 0)
					{
						RemoveChildAt(0);
					}

					Tree.CreateChildrenNodes(this);
					break;
			}

			HasChildren = Children.Count > 0;
			NotifyPropertyChanged(nameof(IsExpandable));
		}

		private void RemoveChildAt(int index)
		{
			var child = Children[index];
			Tree.DropChildrenRows(child, true);
			ClearChildrenSource(child);
			Children.RemoveAt(index);
		}

		private void ClearChildrenSource(TreeNode node)
		{
			node.ChildrenSource = null;

			foreach (TreeNode child in node.Children)
			{
				ClearChildrenSource(child);
			}
		}
	}
}
