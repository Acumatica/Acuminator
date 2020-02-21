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
		public virtual TResult VisitNode(DacNodeViewModel dac, TInput input) => DefaultVisit(dac, input);
		#endregion

		#region Categories
		public virtual TResult VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory, TInput input) => DefaultVisit(dacPropertiesCategory, input);

		public virtual TResult VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory, TInput input) => DefaultVisit(dacKeysCategory, input);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PropertyNodeViewModel property, TInput input) => DefaultVisit(property, input);
		#endregion
	}
}
