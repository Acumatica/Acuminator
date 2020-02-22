using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		protected TResult DefaultValue { get; }

		protected CodeMapTreeVisitor(TResult defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public virtual TResult VisitNode(TreeNodeViewModel node) =>
			node != null
				? node.AcceptVisitor(this)
				: DefaultValue;

		public virtual TResult DefaultVisit(TreeNodeViewModel node) => DefaultValue;

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
	}
}
