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
		public virtual TResult VisitNode(AllDacFieldsDacCategoryNodeViewModel dacPropertiesCategory, TInput input) => DefaultVisit(dacPropertiesCategory, input);

		public virtual TResult VisitNode(KeyDacFieldsCategoryNodeViewModel dacKeysCategory, TInput input) => DefaultVisit(dacKeysCategory, input);

		public virtual TResult VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory, TInput input) =>
			DefaultVisit(dacInitializationAndActivationCategory, input);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(DacFieldGroupingNodeViewModel dacField, TInput input) => 
			DefaultVisit(dacField, input);

		public virtual TResult VisitNode(IsActiveDacMethodNodeViewModel isActiveDacMethodNode, TInput input) =>
			DefaultVisit(isActiveDacMethodNode, input);

		public virtual TResult VisitNode(DacBqlFieldNodeViewModel dacBqlField, TInput input) => 
			DefaultVisit(dacBqlField, input);

		public virtual TResult VisitNode(DacFieldPropertyNodeViewModel dacFieldProperty, TInput input) => 
			DefaultVisit(dacFieldProperty, input);
		#endregion

		#region Attribute Nodes
		public virtual TResult VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode, TInput input) => DefaultVisit(attributeGroupNode, input);

		public virtual TResult VisitNode(DacFieldAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);

		public virtual TResult VisitNode(DacAttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);
		#endregion
	}
}
