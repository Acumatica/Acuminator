using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which produces a result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		protected const int MaxUncheckedRecursionDepth = 20;
		private int _recursionDepth;

		protected TResult DefaultValue { get; }

		protected CodeMapTreeVisitor(TResult defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public virtual TResult VisitNode(TreeNodeViewModel node)
		{
			if (node == null)
				return DefaultValue;

			_recursionDepth++;

			try
			{
				StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
				return node.AcceptVisitor(this);
			}
			finally
			{
				_recursionDepth--;
			}	
		}

		public virtual TResult DefaultVisit(TreeNodeViewModel nodeViewModel) => DefaultValue;

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
	}
}
