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

		public virtual TResult VisitNode(AttributeNodeViewModel attributeNode, CancellationToken cancellation) => DefaultValue;
	}
}
