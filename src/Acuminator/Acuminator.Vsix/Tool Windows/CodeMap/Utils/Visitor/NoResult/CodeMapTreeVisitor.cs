using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor which doesn't produce result.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor
	{
		public virtual void VisitNode(TreeNodeViewModel node)
		{
			if (node != null)
			{
				node.AcceptVisitor(this);
			}
		}

		public virtual void DefaultVisit(TreeNodeViewModel nodeViewModel) { }

		public virtual void VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);
	}
}
