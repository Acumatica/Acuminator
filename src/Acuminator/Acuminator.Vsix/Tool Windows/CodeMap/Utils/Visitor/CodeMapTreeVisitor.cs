using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Base class for code map tree visitor.
	/// </summary>
	public abstract partial class CodeMapTreeVisitor<TResult>
	{
		protected TResult DefaultValue { get; }

		protected CodeMapTreeVisitor(TResult defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public virtual TResult Visit(TreeNodeViewModel nodeViewModel) =>
			nodeViewModel != null
				? nodeViewModel.AcceptVisitor(this)
				: DefaultVisit(nodeViewModel);

		public virtual TResult DefaultVisit(TreeNodeViewModel nodeViewModel) => DefaultValue;

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode) => DefaultVisit(attributeNode);	
	}
}
