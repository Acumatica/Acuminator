using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree walker traversing the code map tree.
	/// </summary>
	public abstract class CodeMapTreeWalker : CodeMapTreeVisitor
	{
		private int _recursionDepth;

		public override void VisitNode(TreeNodeViewModel node)
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

		public override void DefaultVisit(TreeNodeViewModel node) 
		{
			if (node == null || node.Children.Count == 0)
				return;

			foreach (TreeNodeViewModel child in node.Children)
			{
				VisitNode(child);
			}
		}
	}
}
