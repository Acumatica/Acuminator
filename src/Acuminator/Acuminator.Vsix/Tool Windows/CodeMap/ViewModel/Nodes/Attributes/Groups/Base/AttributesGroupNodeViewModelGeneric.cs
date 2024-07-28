#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class AttributesGroupNodeViewModel<TAttributeInfo> : AttributesGroupNodeViewModel
	where TAttributeInfo : AttributeInfoBase
	{
		protected AttributesGroupNodeViewModel(TreeNodeViewModel parent, bool isExpanded = false) : base(parent, isExpanded)
		{
		}

		public sealed override IEnumerable<AttributeInfoBase> UntypedAttributeInfos() => AttributeInfos();

		public abstract IEnumerable<TAttributeInfo> AttributeInfos();
	}
}
