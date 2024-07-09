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

			var defaultNavigationAttribute = pxContext.CheckIfNull().AttributeTypes.PXPrimaryGraphBaseAttribute ?? 
											 pxContext.AttributeTypes.PXPrimaryGraphAttribute;
			return attributeType.InheritsFromOrEquals(defaultNavigationAttribute);
		}
	}
}
