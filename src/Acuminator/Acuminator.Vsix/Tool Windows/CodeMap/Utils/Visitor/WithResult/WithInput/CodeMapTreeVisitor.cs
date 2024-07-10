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
		protected TResult DefaultValue { get; }

		protected CodeMapTreeVisitor(TResult defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public virtual TResult VisitNode(TreeNodeViewModel node, TInput input) =>
			node != null
				? node.AcceptVisitor(this, input)
				: DefaultValue;

		public virtual TResult DefaultVisit(TreeNodeViewModel node, TInput input) => DefaultValue;
	}
}
