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

		/// <summary>
		/// Check if the <paramref name="methodSymbol"/> object has an attribute of a given <paramref name="attributeType"/>.
		/// </summary>
		/// <param name="methodSymbol">The method to act on.</param>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="checkOverrides">True to check method overrides.</param>
		/// <param name="checkForDerivedAttributes">(Optional) True to check for attributes derived from <paramref name="attributeType"/>.</param>
		/// <returns>
		/// True if method has attribute of <paramref name="attributeType"/>, false if not.
		/// </returns>
		public static bool HasAttribute(this IMethodSymbol methodSymbol, INamedTypeSymbol attributeType, bool checkOverrides, 
										bool checkForDerivedAttributes = true)
		{
			methodSymbol.ThrowOnNull(nameof(methodSymbol));
			attributeType.ThrowOnNull(nameof(attributeType));

			Func<IMethodSymbol, bool> attributeCheck = checkForDerivedAttributes
				? (Func<IMethodSymbol, bool>)HasDerivedAttribute
				: HasAttribute;

			if (attributeCheck(methodSymbol))
				return true;
			
			if (checkOverrides && methodSymbol.IsOverride)
			{
				var overrides = methodSymbol.GetOverrides();
				return overrides.Any(attributeCheck);
			}

			return false;

			//-----------------------------------------------------------
			bool HasAttribute(IMethodSymbol methodToCheck) =>
				methodToCheck.GetAttributes()
							 .Any(a => a.AttributeClass.Equals(attributeType));

			bool HasDerivedAttribute(IMethodSymbol methodToCheck) =>
				methodToCheck.GetAttributes()
							 .Any(a => a.AttributeClass.InheritsFrom(attributeType));
		}

		/// <summary>
		/// Gets the <paramref name="methodSymbol"/> and its overrides.
		/// </summary>
		/// <param name="methodSymbol">The method to act on.</param>
		/// <returns>
		/// The <paramref name="methodSymbol"/> and its overrides.
		/// </returns>
		public static IEnumerable<IMethodSymbol> GetOverridesAndThis(this IMethodSymbol methodSymbol) =>
			GetOverridesImpl(methodSymbol.CheckIfNull(nameof(methodSymbol)), includeThis: true);

		/// <summary>
		/// Gets the overrides of <paramref name="methodSymbol"/>.
		/// </summary>
		/// <param name="methodSymbol">The method to act on.</param>
		/// <returns>
		/// The overrides of <paramref name="methodSymbol"/>.
		/// </returns>
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
