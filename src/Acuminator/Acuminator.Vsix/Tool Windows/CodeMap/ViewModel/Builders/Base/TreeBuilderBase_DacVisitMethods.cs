using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree builder.
	/// </summary>
	public abstract partial class TreeBuilderBase
	{
		#region Roots
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacNodeViewModel dac, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Categories
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacPropertiesCategoryNodeViewModel dacPropertiesCategory,
																				bool expandChildren, CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion

		#region Leaf Nodes
		public virtual IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(PropertyNodeViewModel property, bool expandChildren,
																				CancellationToken cancellation) =>
			Enumerable.Empty<TreeNodeViewModel>();
		#endregion
	}
}
