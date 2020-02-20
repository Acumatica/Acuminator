using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		protected const int MaxUncheckedRecursionDepth = 20;
		private int _recursionDepth;

		public virtual void Visit(TreeNodeViewModel node)
		{
			if (node == null)
				return;

			_recursionDepth++;

			try
			{
				StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
				node.AcceptVisitor(this);
			}
			finally
			{
				_recursionDepth--;
			}	
		}

		public virtual void DefaultVisit(TreeNodeViewModel nodeViewModel) { }

		public virtual void VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
	}
}
