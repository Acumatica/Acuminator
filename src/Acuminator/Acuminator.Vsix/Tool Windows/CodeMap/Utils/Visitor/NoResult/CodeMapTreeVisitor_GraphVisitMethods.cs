using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		#region Roots
		public virtual void VisitNode(GraphNodeViewModel graph) => DefaultVisit(graph);
		#endregion

		#region Categories
		public virtual void VisitNode(ActionCategoryNodeViewModel actionCategory) => DefaultVisit(actionCategory);

		public virtual void VisitNode(ViewCategoryNodeViewModel viewCategory) => DefaultVisit(viewCategory);

		public virtual void VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory) => DefaultVisit(cacheAttachedCategory);

		public virtual void VisitNode(RowEventCategoryNodeViewModel rowEventCategory) => DefaultVisit(rowEventCategory);

		public virtual void VisitNode(FieldEventCategoryNodeViewModel rowEventCategory) => DefaultVisit(rowEventCategory);

		public virtual void VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory) => DefaultVisit(pxOverridesCategory);

		public virtual void VisitNode(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategory) =>
			DefaultVisit(graphInitializationAndActivationCategory);

		public virtual void VisitNode(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategory) =>
			DefaultVisit(graphBaseMemberOverridesCategory);
		#endregion

		#region DAC Grouping
		public virtual void VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual void VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual void VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode) => DefaultVisit(dacGroupingNode);

		public virtual void VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode) => DefaultVisit(dacFieldGroupingNode);
		#endregion

		#region Leaf Nodes
		public virtual void VisitNode(PXOverrideNodeViewModel pxOverrideNode) => DefaultVisit(pxOverrideNode);

		public virtual void VisitNode(ActionNodeViewModel actionNode) => DefaultVisit(actionNode);

		public virtual void VisitNode(ViewNodeViewModel viewNode) => DefaultVisit(viewNode);

		public virtual void VisitNode(CacheAttachedNodeViewModel cacheAttachedNode) => DefaultVisit(cacheAttachedNode);

		public virtual void VisitNode(RowEventNodeViewModel rowEventNode) => DefaultVisit(rowEventNode);

		public virtual void VisitNode(FieldEventNodeViewModel fieldEventNode) => DefaultVisit(fieldEventNode);

		public virtual void VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo) => DefaultVisit(graphMemberInfo);

		public virtual void VisitNode(IsActiveGraphMethodNodeViewModel isActiveGraphMethodNode) =>
			DefaultVisit(isActiveGraphMethodNode);

		public virtual void VisitNode(GraphInstanceConstructorNodeViewModel graphInstanceConstructorNode) =>
			DefaultVisit(graphInstanceConstructorNode);

		public virtual void VisitNode(GraphStaticConstructorNodeViewModel graphInstanceConstructorNode) =>
			DefaultVisit(graphInstanceConstructorNode);

		public virtual void VisitNode(GraphBaseMembeOverrideNodeViewModel graphBaseMembeOverrideNode) =>
			DefaultVisit(graphBaseMembeOverrideNode);
		#endregion
	}
}
