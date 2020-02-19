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
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		#region Roots
		public virtual TResult VisitNode(GraphNodeViewModel graph, CancellationToken cancellation) => default;
		#endregion

		#region Categories
		public virtual TResult VisitNode(ActionCategoryNodeViewModel actionCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(ViewCategoryNodeViewModel viewCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(RowEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(FieldEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory, CancellationToken cancellation) => default;
		#endregion

		#region DAC Grouping
		public virtual TResult VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode, CancellationToken cancellation) => default;
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PXOverrideNodeViewModel pxOverrideNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(ActionNodeViewModel actionNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(ViewNodeViewModel viewNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(CacheAttachedNodeViewModel cacheAttachedNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(RowEventNodeViewModel rowEventNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(FieldEventNodeViewModel fieldEventNode, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo, CancellationToken cancellation) => default;
		#endregion
	}
}
