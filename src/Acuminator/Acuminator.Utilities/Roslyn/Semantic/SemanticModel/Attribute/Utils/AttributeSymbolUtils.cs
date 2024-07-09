using System;
using System.Collections.Generic;
using System.Text;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	public static class AttributeSymbolUtils
	{
		public static bool IsDefaultNavigation(this INamedTypeSymbol attributeType, PXContext pxContext)
		{
			attributeType.ThrowOnNull();

			if (pxContext.CheckIfNull().AttributeTypes.PXPrimaryGraphBaseAttribute is INamedTypeSymbol pxPrimaryGraphBaseAttribute)
				return attributeType.InheritsFromOrEquals(pxPrimaryGraphBaseAttribute);
			else
				return attributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXPrimaryGraphAttribute);
		}
	}
}
