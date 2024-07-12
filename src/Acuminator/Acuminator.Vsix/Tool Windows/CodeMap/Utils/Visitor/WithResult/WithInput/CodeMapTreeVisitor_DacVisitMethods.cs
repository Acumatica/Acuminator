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
		public virtual TResult VisitNode(DacNodeViewModel dac, TInput input) => DefaultVisit(dac, input);
		#endregion

		#region Categories
		public virtual TResult VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory, TInput input) => DefaultVisit(dacPropertiesCategory, input);

		public virtual TResult VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory, TInput input) => DefaultVisit(dacKeysCategory, input);

		public virtual TResult VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory, TInput input) =>
			DefaultVisit(dacInitializationAndActivationCategory, input);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PropertyNodeViewModel property, TInput input) => DefaultVisit(property, input);

		public virtual TResult VisitNode(IsActiveDacMethodNodeViewModel isActiveDacMethodNode, TInput input) =>
			DefaultVisit(isActiveDacMethodNode, input);
		#endregion

		#region Attribute Nodes
		public virtual TResult VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode, TInput input) => DefaultVisit(attributeGroupNode, input);

		public virtual TResult VisitNode(DacFieldAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);

		public virtual TResult VisitNode(DacAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);
		#endregion
	}
}
