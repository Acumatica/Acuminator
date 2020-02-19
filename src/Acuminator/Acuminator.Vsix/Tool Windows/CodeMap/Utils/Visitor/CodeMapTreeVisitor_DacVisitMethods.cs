using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		#region Roots
		public virtual TResult VisitNode(DacNodeViewModel dac) => DefaultVisit(dac);
		#endregion

		#region Categories
		public virtual TResult VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory) => DefaultVisit(dacPropertiesCategory);

		public virtual TResult VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory) => DefaultVisit(dacKeysCategory);
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PropertyNodeViewModel property) => DefaultVisit(property);
		#endregion
	}
}
