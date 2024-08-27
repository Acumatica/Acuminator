#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		public virtual void VisitNode(TreeNodeViewModel node)
		{
			node?.AcceptVisitor(this);
		}

		public virtual void DefaultVisit(TreeNodeViewModel nodeViewModel) { }
	}
}
