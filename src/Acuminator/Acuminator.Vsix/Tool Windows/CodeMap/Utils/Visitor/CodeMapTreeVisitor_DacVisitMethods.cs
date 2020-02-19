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
	public abstract partial class CodeMapTreeVisitor
	{
		#region Roots
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacNodeViewModel dac, CancellationToken cancellation) => Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Categories
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory, CancellationToken cancellation) => Enumerable.Empty<TreeNodeViewModel>();

		public virtual IEnumerable<TreeNodeViewModel> VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory, CancellationToken cancellation) => Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Leaf Nodes
		public virtual IEnumerable<TreeNodeViewModel> VisitNode(PropertyNodeViewModel property, CancellationToken cancellation) => Enumerable.Empty<TreeNodeViewModel>();
		#endregion
	}
}
