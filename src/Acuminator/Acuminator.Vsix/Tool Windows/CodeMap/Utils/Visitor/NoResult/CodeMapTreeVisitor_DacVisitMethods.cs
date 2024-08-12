#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		#region Roots
		public virtual void VisitNode(DacNodeViewModel dac) => DefaultVisit(dac);
		#endregion

		#region Categories
		public virtual void VisitNode(AllDacFieldsDacCategoryNodeViewModel allDacFieldsCategory) => DefaultVisit(allDacFieldsCategory);

		public virtual void VisitNode(KeyDacFieldsCategoryNodeViewModel dacKeyFieldsCategory) => DefaultVisit(dacKeyFieldsCategory);

		public virtual void VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory) =>
			DefaultVisit(dacInitializationAndActivationCategory);
		#endregion

		#region Leaf Nodes
		public virtual void VisitNode(DacFieldGroupingNodeViewModel dacField) => DefaultVisit(dacField);

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
