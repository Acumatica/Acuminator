﻿#nullable enable

using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		#region Roots
		public virtual void VisitNode(DacNodeViewModel dac) => DefaultVisit(dac);

		public virtual void VisitNode(BaseDacPlaceholderNodeViewModel baseDac) => DefaultVisit(baseDac);
		#endregion

		#region Categories
		public virtual void VisitNode(AllDacFieldsDacCategoryNodeViewModel allDacFieldsCategory) => DefaultVisit(allDacFieldsCategory);

		public virtual void VisitNode(KeyDacFieldsCategoryNodeViewModel dacKeyFieldsCategory) => DefaultVisit(dacKeyFieldsCategory);

		public virtual void VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory) =>
			DefaultVisit(dacInitializationAndActivationCategory);

		public virtual void VisitNode(DacBaseTypesCategoryNodeViewModel dacBaseTypesCategory) => DefaultVisit(dacBaseTypesCategory);
		#endregion

		#region Leaf Nodes
		public virtual void VisitNode(DacFieldNodeViewModel dacField) => DefaultVisit(dacField);

		public virtual void VisitNode(IsActiveDacMethodNodeViewModel isActiveDacMethodNode) =>
		DefaultVisit(isActiveDacMethodNode);

		public virtual void VisitNode(DacBqlFieldNodeViewModel dacBqlField) => DefaultVisit(dacBqlField);

		public virtual void VisitNode(DacFieldPropertyNodeViewModel dacFieldProperty) => DefaultVisit(dacFieldProperty);
		#endregion

		#region Attribute Nodes
		public virtual void VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode) => DefaultVisit(attributeGroupNode);

		public virtual void VisitNode(DacFieldAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);

		public virtual void VisitNode(DacAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
		#endregion
	}
}
