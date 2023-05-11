#nullable enable

using System.Collections.Generic;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	public static class PXOverrideExtensions
	{
		internal static INamedTypeSymbol? GetFirstTypeArgument(this INamedTypeSymbol type)
		{
			if (type == null)
			{
				return null;
			}

			if (type.TypeArguments.Any())
			{
				return type.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
			}

			return type.ContainingType.GetFirstTypeArgument();
		}

		internal static INamedTypeSymbol? GetPXGraphExtension(this INamedTypeSymbol typeSymbol, INamedTypeSymbol pxGraphExtensionType)
		{
			if (typeSymbol == null)
			{
				return null;
			}

			if (typeSymbol.InheritsFromOrEquals(pxGraphExtensionType) && typeSymbol.IsGenericType)
			{
				return typeSymbol;
			}

			return typeSymbol.BaseType.GetPXGraphExtension(pxGraphExtensionType);
		}

		internal static void GetBaseTypes(
			this INamedTypeSymbol typeSymbol,
			INamedTypeSymbol pxGraphExtensionType,
			HashSet<INamedTypeSymbol> allBaseTypes)
		{
			if (typeSymbol == null || typeSymbol.SpecialType != SpecialType.None)
			{
				return;
			}

			if (!allBaseTypes.Add(typeSymbol))
			{
				return;
			}

			var extensionType = typeSymbol.BaseType.GetPXGraphExtension(pxGraphExtensionType);
			var graphType = extensionType?.GetFirstTypeArgument();

			if (graphType != null)
			{
				graphType.GetBaseTypes(pxGraphExtensionType, allBaseTypes);
			}

			typeSymbol.BaseType.GetBaseTypes(pxGraphExtensionType, allBaseTypes);
		}
	}
}
