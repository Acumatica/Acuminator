#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CacheAttachedAttributeNodeViewModel : AttributeNodeViewModel<CacheAttachedAttributeInfo>
	{
		public CacheAttachedAttributeNodeViewModel(TreeNodeViewModel nodeVM, CacheAttachedAttributeInfo attributeInfo, bool isExpanded = false) :
												   base(nodeVM, attributeInfo, isExpanded)
		{
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
