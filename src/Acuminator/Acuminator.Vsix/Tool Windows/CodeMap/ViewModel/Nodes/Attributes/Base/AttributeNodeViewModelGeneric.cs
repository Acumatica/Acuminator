#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class AttributeNodeViewModel<TAttributeInfo> : AttributeNodeViewModel
	where TAttributeInfo : AttributeInfoBase
	{
		public new TAttributeInfo AttributeInfo => (TAttributeInfo)base.AttributeInfo;

		protected AttributeNodeViewModel(TreeNodeViewModel nodeVM, TAttributeInfo attributeInfo, bool isExpanded = false) :
									base(nodeVM, attributeInfo, isExpanded)
		{
		}
	}
}
