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
		public virtual void VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory) => DefaultVisit(dacPropertiesCategory);

		public virtual void VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory) => DefaultVisit(dacKeysCategory);

		public virtual void VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory) =>
			DefaultVisit(dacInitializationAndActivationCategory);
		#endregion

		#region Leaf Nodes
		public virtual void VisitNode(PropertyNodeViewModel property) => DefaultVisit(property);

		public virtual void VisitNode(IsActiveDacMethodNodeViewModel isActiveDacMethodNode) =>
			DefaultVisit(isActiveDacMethodNode);
		#endregion

		#region Attribute Nodes
		public virtual void VisitNode(DacAttributesGroupNodeViewModel attributeNode) => DefaultVisit(attributeNode);

		public virtual void VisitNode(DacFieldAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);

		public virtual void VisitNode(DacAttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
		#endregion
	}
}
