using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		#region Roots
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(GraphNodeViewModel graph, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Categories
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(ActionCategoryNodeViewModel actionCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(ViewCategoryNodeViewModel viewCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(RowEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(FieldEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region DAC Grouping
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Leaf Nodes
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(PXOverrideNodeViewModel pxOverrideNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(ActionNodeViewModel actionNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(ViewNodeViewModel viewNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedNodeViewModel cacheAttachedNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(RowEventNodeViewModel rowEventNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(FieldEventNodeViewModel fieldEventNode, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion
	}
}
