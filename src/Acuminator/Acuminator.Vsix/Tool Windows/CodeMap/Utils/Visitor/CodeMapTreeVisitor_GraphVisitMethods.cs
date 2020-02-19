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
		public virtual TResult VisitNode(GraphNodeViewModel graph, CancellationToken cancellation) => DefaultValue;
		#endregion

		#region Categories
		public virtual TResult VisitNode(ActionCategoryNodeViewModel actionCategory, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(ViewCategoryNodeViewModel viewCategory, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(RowEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(FieldEventCategoryNodeViewModel rowEventCategory, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory, CancellationToken cancellation) => DefaultValue;
		#endregion

		#region DAC Grouping
		public virtual TResult VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode, CancellationToken cancellation) => DefaultValue;
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PXOverrideNodeViewModel pxOverrideNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(ActionNodeViewModel actionNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(ViewNodeViewModel viewNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(CacheAttachedNodeViewModel cacheAttachedNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(RowEventNodeViewModel rowEventNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(FieldEventNodeViewModel fieldEventNode, CancellationToken cancellation) => DefaultValue;

		public virtual TResult VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo, CancellationToken cancellation) => DefaultValue;
		#endregion
	}
}
