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
		public virtual TResult VisitNode(GraphNodeViewModel graph) => DefaultVisit(graph);
		#endregion

		#region Categories
		public virtual TResult VisitNode(ActionCategoryNodeViewModel actionCategory) => DefaultVisit(actionCategory);

		public virtual TResult VisitNode(ViewCategoryNodeViewModel viewCategory) => DefaultVisit(viewCategory);

		public virtual TResult VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory) => DefaultVisit(cacheAttachedCategory);

		public virtual TResult VisitNode(RowEventCategoryNodeViewModel rowEventCategory) => DefaultVisit(rowEventCategory);

		public virtual TResult VisitNode(FieldEventCategoryNodeViewModel rowEventCategory) => DefaultVisit(rowEventCategory);

		public virtual TResult VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory) => DefaultVisit(pxOverridesCategory);
		#endregion

		#region DAC Grouping
		public virtual TResult VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual TResult VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual TResult VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual TResult VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode) => DefaultVisit(dacFieldGroupingNode);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PXOverrideNodeViewModel pxOverrideNode) => DefaultVisit(pxOverrideNode);

		public virtual TResult VisitNode(ActionNodeViewModel actionNode) => DefaultVisit(actionNode);

		public virtual TResult VisitNode(ViewNodeViewModel viewNode) => DefaultVisit(viewNode);

		public virtual TResult VisitNode(CacheAttachedNodeViewModel cacheAttachedNode) => DefaultVisit(cacheAttachedNode);

		public virtual TResult VisitNode(RowEventNodeViewModel rowEventNode) => DefaultVisit(rowEventNode);

		public virtual TResult VisitNode(FieldEventNodeViewModel fieldEventNode) => DefaultVisit(fieldEventNode);

		public virtual TResult VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo) => DefaultVisit(graphMemberInfo);
		#endregion
	}
}
