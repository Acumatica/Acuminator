#nullable enable

using System;
using System.Collections.Generic;

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

		public static string? GetNameFromPXCacheNameAttribute(this AttributeData attributeData)
		{
			var constructorArgs = attributeData.CheckIfNull().ConstructorArguments;

			if (constructorArgs.Length != 1)
				return null;

			var nameArg = constructorArgs[0];

			if (nameArg.Kind != TypedConstantKind.Primitive)
				return null;

			return nameArg.Value as string;
		}
	}
}
