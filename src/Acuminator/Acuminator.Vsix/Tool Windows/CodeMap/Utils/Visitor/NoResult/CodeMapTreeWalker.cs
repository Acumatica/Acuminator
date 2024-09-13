#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree walker traversing the code map tree.
	/// </summary>
	public abstract class CodeMapTreeWalker : CodeMapTreeVisitor
	{
		private int _recursionDepth;

		protected abstract bool VisitOnlyDisplayedNodes { get; }

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
			if (node == null)
				return;

			IReadOnlyCollection<TreeNodeViewModel> nodesToVisit = VisitOnlyDisplayedNodes
				? node.DisplayedChildren
				: node.AllChildren;

			if (nodesToVisit.Count > 0)
			{
				foreach (TreeNodeViewModel child in nodesToVisit)
				{
					VisitNode(child);
				}
			}
		}
	}
}
