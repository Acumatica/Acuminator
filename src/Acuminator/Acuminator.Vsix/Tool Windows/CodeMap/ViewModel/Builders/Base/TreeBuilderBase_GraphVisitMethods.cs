using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract partial class TreeBuilderBase
	{
		#region Roots
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(GraphNodeViewModel graph, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Categories
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ActionCategoryNodeViewModel actionCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ViewCategoryNodeViewModel viewCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(RowEventCategoryNodeViewModel rowEventCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(FieldEventCategoryNodeViewModel rowEventCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(PXOverridesCategoryNodeViewModel pxOverridesCategory, bool expandChildren,
																			CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region DAC Grouping
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacEventsGroupingNodeBaseViewModel dacEventsGroupingNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacFieldEventsGroupingNodeViewModel dacFieldEventsGroupingNode,
																				bool expandChildren, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Leaf Nodes
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(PXOverrideNodeViewModel pxOverrideNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ActionNodeViewModel actionNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ViewNodeViewModel viewNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(CacheAttachedNodeViewModel cacheAttachedNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(RowEventNodeViewModel rowEventNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(FieldEventNodeViewModel fieldEventNode, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(GraphMemberInfoNodeViewModel graphMemberInfo, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion
	}
}
