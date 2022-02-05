using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which accepts an input and produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TInput, TResult>
	{
		#region Roots
		public virtual TResult VisitNode(GraphNodeViewModel graph, TInput input) => DefaultVisit(graph, input);
		#endregion

		#region Categories
		public virtual TResult VisitNode(ActionCategoryNodeViewModel actionCategory, TInput input) => DefaultVisit(actionCategory, input);

		public virtual TResult VisitNode(ViewCategoryNodeViewModel viewCategory, TInput input) => DefaultVisit(viewCategory, input);

		public virtual TResult VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, TInput input) => DefaultVisit(cacheAttachedCategory, input);

		public virtual TResult VisitNode(RowEventCategoryNodeViewModel rowEventCategory, TInput input) => DefaultVisit(rowEventCategory, input);

		public virtual TResult VisitNode(FieldEventCategoryNodeViewModel rowEventCategory, TInput input) => DefaultVisit(rowEventCategory, input);

		public virtual TResult VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory, TInput input) => DefaultVisit(pxOverridesCategory, input);

		public virtual TResult VisitNode(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategory, TInput input) =>
			DefaultVisit(graphInitializationAndActivationCategory, input);
		#endregion

		#region DAC Grouping
		public virtual TResult VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode, TInput input) => DefaultVisit(dacFieldGroupingNode, input);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PXOverrideNodeViewModel pxOverrideNode, TInput input) => DefaultVisit(pxOverrideNode, input);

		public virtual TResult VisitNode(ActionNodeViewModel actionNode, TInput input) => DefaultVisit(actionNode, input);

		public virtual TResult VisitNode(ViewNodeViewModel viewNode, TInput input) => DefaultVisit(viewNode, input);

		public virtual TResult VisitNode(CacheAttachedNodeViewModel cacheAttachedNode, TInput input) => DefaultVisit(cacheAttachedNode, input);

		public virtual TResult VisitNode(RowEventNodeViewModel rowEventNode, TInput input) => DefaultVisit(rowEventNode, input);

		public virtual TResult VisitNode(FieldEventNodeViewModel fieldEventNode, TInput input) => DefaultVisit(fieldEventNode, input);

		public virtual TResult VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo, TInput input) => DefaultVisit(graphMemberInfo, input);

		public virtual TResult VisitNode(IsActiveGraphMethodNodeViewModel isActiveGraphMethodNode, TInput input) =>
			DefaultVisit(isActiveGraphMethodNode, input);
		#endregion
	}
}
