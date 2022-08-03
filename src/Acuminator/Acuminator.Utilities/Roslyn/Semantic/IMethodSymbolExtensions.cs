#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class IMethodSymbolExtensions
	{
		public static bool IsInstanceConstructor(this IMethodSymbol methodSymbol)
		{
			methodSymbol.ThrowOnNull(nameof (methodSymbol));

			return !methodSymbol.IsStatic && methodSymbol.MethodKind == MethodKind.Constructor;
		}

		public static IEnumerable<IMethodSymbol> GetOverridesAndThis(this IMethodSymbol methodSymbol) =>
			GetOverridesImpl(methodSymbol.CheckIfNull(nameof(methodSymbol)), includeThis: true);

		public static IEnumerable<IMethodSymbol> GetOverrides(this IMethodSymbol methodSymbol) =>
			GetOverridesImpl(methodSymbol.CheckIfNull(nameof(methodSymbol)), includeThis: false);

		private static IEnumerable<IMethodSymbol> GetOverridesImpl(IMethodSymbol methodSymbol, bool includeThis)
		{
			if (!methodSymbol.IsOverride)
			{
				if (includeThis)
					yield return methodSymbol;

				yield break;
			}
			else
			{
				IMethodSymbol current = includeThis ? methodSymbol : methodSymbol.OverriddenMethod;

				while (current?.IsOverride == true)
				{
					yield return current;
					current = current.OverriddenMethod;
				}
			}
		}
	}
}
