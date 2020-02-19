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
		public virtual TResult VisitNode(DacNodeViewModel dac, CancellationToken cancellation) => default;
		#endregion

		#region Categories
		public virtual TResult VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory, CancellationToken cancellation) => default;

		public virtual TResult VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory, CancellationToken cancellation) => default;
		#endregion

		#region Leaf Nodes
		public virtual TResult VisitNode(PropertyNodeViewModel property, CancellationToken cancellation) => default;
		#endregion
	}
}
