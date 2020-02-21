using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which accepts an input and produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TInput, TResult>
	{
		protected TResult DefaultValue { get; }

		protected CodeMapVisitor(TResult defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public virtual TResult VisitNode(TreeNodeViewModel node, TInput input) =>
			node != null
				? node.AcceptVisitor(this, input)
				: DefaultValue;

		public virtual TResult DefaultVisit(TreeNodeViewModel node, TInput input) => DefaultValue;

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode, TInput input) => DefaultVisit(attributeNode, input);
	}
}
