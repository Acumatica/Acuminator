#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		#region Roots
		public virtual TResult VisitNode(DacNodeViewModel dac) => DefaultVisit(dac);

		public virtual TResult VisitNode(BaseDacPlaceholderNodeViewModel baseDac) => DefaultVisit(baseDac);
		#endregion

		#region Categories
		public virtual TResult VisitNode(AllDacFieldsDacCategoryNodeViewModel allDacFieldsCategory) => DefaultVisit(allDacFieldsCategory);

		public virtual TResult VisitNode(KeyDacFieldsCategoryNodeViewModel dacKeyFieldsCategory) => DefaultVisit(dacKeyFieldsCategory);

		public virtual TResult VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory) =>
			DefaultVisit(dacInitializationAndActivationCategory);

		public virtual TResult VisitNode(DacBaseTypesCategoryNodeViewModel dacBaseTypesCategory) => DefaultVisit(dacBaseTypesCategory);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(DacFieldNodeViewModel dacField) => DefaultVisit(dacField);

		public virtual TResult VisitNode(IsActiveDacMethodNodeViewModel isActiveDacMethodNode) =>
			DefaultVisit(isActiveDacMethodNode);

		public virtual TResult VisitNode(DacBqlFieldNodeViewModel dacBqlField) => DefaultVisit(dacBqlField);

		public virtual TResult VisitNode(DacFieldPropertyNodeViewModel dacFieldProperty) => DefaultVisit(dacFieldProperty);
		#endregion

		#region Attribute Nodes
		public virtual TResult VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode) => DefaultVisit(attributeGroupNode);

		public virtual TResult VisitNode(DacFieldAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);

		public virtual TResult VisitNode(DacAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
		#endregion
	}
}
