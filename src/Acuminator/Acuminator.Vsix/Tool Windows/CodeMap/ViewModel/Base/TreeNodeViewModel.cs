#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.Utilities;

using Microsoft.VisualStudio.PlatformUI;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract partial class TreeNodeViewModel : ViewModelBase
	{
		public TreeViewModel Tree { get; }

		public abstract string Name { get; protected set; }

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

		public ExtendedObservableCollection<TreeNodeViewModel> AllChildren { get; } = new();

		private readonly ExtendedObservableCollection<TreeNodeViewModel> _mutableDisplayedChildren = new();

		public ReadOnlyObservableCollection<TreeNodeViewModel> DisplayedChildren { get; }

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

					DisplayedDescendants().ForEach(node => node.NotifyPropertyChanged(nameof(AreDetailsVisible)));
				}
			}
		}

		protected bool _isVisible;

		/// <summary>
		/// Is node visible. This is internal property that works only for the node itself and doesn't affect its children or parents.
		/// </summary>
		public bool IsVisible
		{
			get => _isVisible;
			protected set 
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

		protected TreeNodeViewModel(TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded)
		{
			Tree 		= tree.CheckIfNull();
			Parent 		= parent;
			_isExpanded = isExpanded;

			AllChildren.CollectionChanged += AllChildren_CollectionChanged;
			DisplayedChildren = new ReadOnlyObservableCollection<TreeNodeViewModel>(_mutableDisplayedChildren);
		}

		public virtual Task NavigateToItemAsync() => Task.CompletedTask;

		/// <summary>
		/// Check if node is visible in filter going from the node to its descendants.
		/// </summary>
		/// <param name="filterOptions">Filter options.</param>
		/// <returns>
		/// True if visible in filter, false if not.
		/// </returns>
		public bool IsVisibleInFilter(FilterOptions? filterOptions)
		{
			if (filterOptions == null || !filterOptions.HasFilter)
				return true;

			switch (FilterBehavior)
			{
				case TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter:
					if (NameMatchesPattern(filterOptions.FilterPattern))
						return true;

					goto case TreeNodeFilterBehavior.DisplayedIfChildrenMeetFilter;

				case TreeNodeFilterBehavior.DisplayedIfChildrenMeetFilter:
					return AllChildren.Count > 0 && AllChildren.Any(childNode => childNode.IsVisibleInFilter(filterOptions));

				case TreeNodeFilterBehavior.AlwaysHidden:
					return false;

				case TreeNodeFilterBehavior.AlwaysDisplayed:
				default:
					return true;
			}
		}

		public virtual bool NameMatchesPattern(string? pattern) => MatchPattern(Name, pattern);

		protected static bool MatchPattern(string stringToMatch, string? pattern) =>
			pattern.IsNullOrEmpty() || stringToMatch.Contains(pattern, StringComparison.OrdinalIgnoreCase);

		public abstract TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input);

		public abstract TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor);

		public abstract void AcceptVisitor(CodeMapTreeVisitor treeVisitor);

		public virtual void ExpandOrCollapseAll(bool expand)
		{
			IsExpanded = expand;
			AllChildren.ForEach(childNode => childNode!.ExpandOrCollapseAll(expand));
		}

		public virtual void OnVsColorThemeChanged(ThemeChangedEventArgs e)
		{
			if (IconDependsOnCurrentTheme)
				NotifyPropertyChanged(nameof(NodeIcon));
		}

		public void RefreshDisplayedChildren()
		{
			var visibleChildren = AllChildren.Where(child => child.IsVisible);
			_mutableDisplayedChildren.Reset(visibleChildren);
		}

		protected bool ShouldShowDetails()
		{
			if (IsRoot)
				return true;

			return IsVisible && Ancestors().All(ancestor => ancestor.IsExpanded && ancestor.IsVisible);
		}

		protected void SubscribeOnDisplayedChildrenCollectionChanged(NotifyCollectionChangedEventHandler collectionChangedEventHandler)
		{
			if (collectionChangedEventHandler != null)
				_mutableDisplayedChildren.CollectionChanged += collectionChangedEventHandler;
		}

		public void RefreshVisibilityForNodeAndSubTreeFromFilter(FilterOptions? filterOptions)
		{
			// Breadth First Search Tree traversal will always order children after their parents,
			// Therefore, the reversal wil always make nodes be processed from children to root
			var nodesAndSelfFromRootToChildren = DescendantsBFS(includeSelf: true, collectOnlyDisplayedNodes: false);

			if (filterOptions == null || !filterOptions.HasFilter)
			{
				FilterHelper.RefreshSubTreeWithoutFilter(this, nodesAndSelfFromRootToChildren);
			}
			else
			{
				FilterHelper.RefreshSubTreeWithFilter(this, nodesAndSelfFromRootToChildren, filterOptions);
			}
		}

		private void AllChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
			RefreshDisplayedChildren();

		#region Tree Traversal Methods
		public IReadOnlyCollection<TreeNodeViewModel> AllDescendantsAndSelf() =>
			AllChildren.Count > 0
				? DescendantsBFS(includeSelf: true, collectOnlyDisplayedNodes: false)
				: [this];

		public IReadOnlyCollection<TreeNodeViewModel> AllDescendants() =>
			AllChildren.Count > 0
				? DescendantsBFS(includeSelf: false, collectOnlyDisplayedNodes: false)
				: [];

		public IEnumerable<TreeNodeViewModel> DisplayedDescendantsAndSelf() =>
			DisplayedChildren.Count > 0
				? DescendantsBFS(includeSelf: true, collectOnlyDisplayedNodes: true)
				: [this];

		public IEnumerable<TreeNodeViewModel> DisplayedDescendants() =>
			DisplayedChildren.Count > 0
				? DescendantsBFS(includeSelf: false, collectOnlyDisplayedNodes: true)
				: [];

		private List<TreeNodeViewModel> DescendantsBFS(bool includeSelf, bool collectOnlyDisplayedNodes)
		{
			// BFS traversal
			IReadOnlyCollection<TreeNodeViewModel> childrenToTraverse = collectOnlyDisplayedNodes
				? DisplayedChildren
				: AllChildren;
			
			var nodesQueue = new List<TreeNodeViewModel>(capacity: childrenToTraverse.Count * 2);
			int currentQueueHeadPosition = 0;

			if (includeSelf)
				nodesQueue.Add(this);
			else if (childrenToTraverse.Count > 0)
				nodesQueue.AddRange(childrenToTraverse);

			while (currentQueueHeadPosition < nodesQueue.Count)
			{
				var currentNode = nodesQueue[currentQueueHeadPosition];
				currentQueueHeadPosition++;

				IReadOnlyCollection<TreeNodeViewModel> currentNodeChildrenToTraverse = collectOnlyDisplayedNodes
					? currentNode.DisplayedChildren
					: currentNode.AllChildren;

				if (currentNodeChildrenToTraverse.Count > 0)
					nodesQueue.AddRange(currentNodeChildrenToTraverse);
			}

			return nodesQueue;
		}

		public IEnumerable<TreeNodeViewModel> AncestorsAndSelf() => IsRoot
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
		#endregion
	}
}
