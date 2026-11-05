#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which accepts an input and produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TInput, TResult>
	{
		#region Roots
		public virtual TResult VisitNode(GraphNodeViewModel graph, TInput input) => DefaultVisit(graph, input);

		public virtual TResult VisitNode(BaseGraphPlaceholderNodeViewModel baseGraph, TInput input) => DefaultVisit(baseGraph, input);
		#endregion

		#region Categories
		public virtual TResult VisitNode(ActionCategoryNodeViewModel actionCategory, TInput input) => DefaultVisit(actionCategory, input);

		public virtual TResult VisitNode(ViewCategoryNodeViewModel viewCategory, TInput input) => DefaultVisit(viewCategory, input);

		public virtual TResult VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory, TInput input) => DefaultVisit(cacheAttachedCategory, input);

		public virtual TResult VisitNode(RowEventHandlerCategoryNodeViewModel rowEventHandlerCategory, TInput input) => DefaultVisit(rowEventHandlerCategory, input);

		public virtual TResult VisitNode(FieldEventHandlerCategoryNodeViewModel fieldEventHandlerCategory, TInput input) => DefaultVisit(fieldEventHandlerCategory, input);

		public virtual TResult VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory, TInput input) => DefaultVisit(pxOverridesCategory, input);

		public virtual TResult VisitNode(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategory, TInput input) =>
			DefaultVisit(graphInitializationAndActivationCategory, input);

		public virtual TResult VisitNode(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategory, TInput input) =>
			DefaultVisit(graphBaseMemberOverridesCategory, input);

		public virtual TResult VisitNode(GraphBaseTypesCategoryNodeViewModel graphBaseTypesCategory, TInput input) => 
			DefaultVisit(graphBaseTypesCategory, input);
		#endregion

		#region DAC Grouping
		public virtual TResult VisitNode(DacGroupingNodeForRowEventHandlerViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacGroupingNodeForCacheAttachedEventHandlerViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacGroupingNodeForFieldEventHandlerViewModel dacGroupingNode, TInput input) => DefaultVisit(dacGroupingNode, input);

		public virtual TResult VisitNode(DacFieldGroupingNodeForFieldEventHandlerViewModel dacFieldGroupingNode, TInput input) => DefaultVisit(dacFieldGroupingNode, input);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PXOverrideNodeViewModel pxOverrideNode, TInput input) => DefaultVisit(pxOverrideNode, input);

		public virtual TResult VisitNode(ActionNodeViewModel actionNode, TInput input) => DefaultVisit(actionNode, input);

		public virtual TResult VisitNode(ViewNodeViewModel viewNode, TInput input) => DefaultVisit(viewNode, input);

		public virtual TResult VisitNode(CacheAttachedNodeViewModel cacheAttachedNode, TInput input) => DefaultVisit(cacheAttachedNode, input);

		public virtual TResult VisitNode(RowEventHandlerNodeViewModel rowEventNode, TInput input) => DefaultVisit(rowEventNode, input);

		public virtual TResult VisitNode(FieldEventHandlerNodeViewModel fieldEventNode, TInput input) => DefaultVisit(fieldEventNode, input);

		public virtual TResult VisitNode(GraphMemberInfoNodeViewModel graphMemberInfo, TInput input) => DefaultVisit(graphMemberInfo, input);

		public virtual TResult VisitNode(IsActiveGraphMethodNodeViewModel isActiveGraphMethodNode, TInput input) =>
			DefaultVisit(isActiveGraphMethodNode, input);

		public virtual TResult VisitNode(IsActiveForGraphMethodNodeViewModel isActiveForGraphMethodNode, TInput input) =>
			DefaultVisit(isActiveForGraphMethodNode, input);

		public virtual TResult VisitNode(GraphConfigureMethodNodeViewModel configureMethodNode, TInput input) =>
			DefaultVisit(configureMethodNode, input);

		public virtual TResult VisitNode(GraphInitializeMethodNodeViewModel initializedMethodNode, TInput input) =>
			DefaultVisit(initializedMethodNode, input);

		public virtual TResult VisitNode(GraphInstanceConstructorNodeViewModel graphInstanceConstructorNode, TInput input) =>
			DefaultVisit(graphInstanceConstructorNode, input);

		public virtual TResult VisitNode(GraphStaticConstructorNodeViewModel graphInstanceConstructorNode, TInput input) =>
			DefaultVisit(graphInstanceConstructorNode, input);

		public virtual TResult VisitNode(GraphBaseMemberOverrideNodeViewModel graphBaseMembeOverrideNode, TInput input) =>
			DefaultVisit(graphBaseMembeOverrideNode, input);
		#endregion

		#region Attribute Nodes
		public virtual TResult VisitNode(GraphAttributesGroupNodeViewModel attributeGroupNode, TInput input) => DefaultVisit(attributeGroupNode, input);

		public virtual TResult VisitNode(CacheAttachedAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);

		public virtual TResult VisitNode(GraphAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);
		#endregion
	}
}
