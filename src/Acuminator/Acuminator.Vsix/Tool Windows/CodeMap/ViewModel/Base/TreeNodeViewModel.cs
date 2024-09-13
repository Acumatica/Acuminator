#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;

using Microsoft.VisualStudio.PlatformUI;

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
		/// The main icon for a node.
		/// </summary>
		public abstract Icon NodeIcon { get; }

		/// <summary>
		/// The icon depends on the current IDE theme.
		/// </summary>
		public virtual bool IconDependsOnCurrentTheme { get; }

		public virtual ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos => null;

		private SortType? _childrenSortType;

		/// <summary>
		/// The sort type of nodes children.
		/// </summary>
		public SortType? ChildrenSortType
		{
			get => _childrenSortType;
			set
			{
				if (_childrenSortType != value)
				{
					_childrenSortType = value;
					NotifyPropertyChanged();
				}
			}
		}

		private SortDirection _childrenSortDirection = SortDirection.Ascending;

		/// <summary>
		/// The children sort direction.
		/// </summary>
		public SortDirection ChildrenSortDirection
		{
			get => _childrenSortDirection;
			set
			{
				if (_childrenSortDirection != value)
				{
					_childrenSortDirection = value;
					NotifyPropertyChanged();
				}
			}
		}

		public abstract TreeNodeFilterBehavior FilterBehavior { get; }

		public TreeNodeViewModel? Parent { get; }

		[MemberNotNullWhen(returnValue: false, nameof(Parent))]
		public bool IsRoot => Parent == null;

		public ExtendedObservableCollection<TreeNodeViewModel> DisplayedChildren { get; } = new ExtendedObservableCollection<TreeNodeViewModel>();

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

					Descendants().ForEach(node => node!.NotifyPropertyChanged(nameof(AreDetailsVisible)));
				}
			}
		}

		protected bool _isVisible;

		public bool IsVisible
		{
			get => _isVisible;
			set 
			{
				if (_isVisible != value)
				{
					_isVisible = value;
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

		private bool _isMouseOver;

		public bool IsMouseOver
		{
			get => _isMouseOver;
			set
			{
				if (_isMouseOver != value)
				{
					_isMouseOver = value;
					NotifyPropertyChanged();
				}
			}
		}

		public virtual bool AreDetailsVisible => ShouldShowDetails();

		protected TreeNodeViewModel(TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded = true)
		{
			Tree = tree.CheckIfNull();
			Parent = parent;
			_isExpanded = isExpanded;
		}

		public virtual Task NavigateToItemAsync() => Task.CompletedTask;

		public abstract TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input);

		public abstract TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor);

		public abstract void AcceptVisitor(CodeMapTreeVisitor treeVisitor);

		public virtual void ExpandOrCollapseAll(bool expand)
		{
			IsExpanded = expand;
			Children.ForEach(childNode => childNode!.ExpandOrCollapseAll(expand));
		}

		public IEnumerable<TreeNodeViewModel> Descendants()
		{
			if (Children.Count == 0)
				yield break;

			foreach (TreeNodeViewModel child in Children)
			{
				yield return child;
				var descendants = child.Descendants();

				foreach (var descendant in descendants)
				{
					yield return descendant;
				}
			}
		}

		public IEnumerable<TreeNodeViewModel> AncestorsOrSelf() => IsRoot
			? [this]
			: Ancestors(includeSelf: true);

		public IEnumerable<TreeNodeViewModel> Ancestors() => IsRoot
			? []
			: Ancestors(includeSelf: false);

		private IEnumerable<TreeNodeViewModel> Ancestors(bool includeSelf)
		{
			TreeNodeViewModel? current = includeSelf ? this : Parent;

			while (current != null)
			{
				yield return current;
				current = current.Parent;
			}
		}

		public virtual void OnVsColorThemeChanged(ThemeChangedEventArgs e) 
		{
			if (IconDependsOnCurrentTheme)
				NotifyPropertyChanged(nameof(NodeIcon));
		}

		protected bool ShouldShowDetails()
		{
			if (IsRoot)
				return true;

			return Ancestors().All(ancestor => ancestor.IsExpanded);
		}
	}
}
