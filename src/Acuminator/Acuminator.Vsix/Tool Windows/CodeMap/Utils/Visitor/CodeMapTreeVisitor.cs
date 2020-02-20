using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor.
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

		public virtual TResult Visit(TreeNodeViewModel node)
		{
			if (node == null)
				return DefaultValue;

			_recursionDepth++;

			try
			{
				EnsureSufficientExecutionStack();
				return node.AcceptVisitor(this);
			}
			finally
			{
				_recursionDepth--;
			}	
		}

		public virtual TResult DefaultVisit(TreeNodeViewModel nodeViewModel) => DefaultValue;

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);

		/// <summary>
		/// Ensures that the remaining stack space is large enough to execute the average function.
		/// </summary>
		/// <param name="recursionDepth">how many times the calling function has recursed</param>
		/// <exception cref="InsufficientExecutionStackException">
		///  The available stack space is insufficient to execute the average function.
		/// </exception>
		[DebuggerStepThrough]
		protected void EnsureSufficientExecutionStack()
		{
			if (_recursionDepth > MaxUncheckedRecursionDepth)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
			}
		}
	}
}
