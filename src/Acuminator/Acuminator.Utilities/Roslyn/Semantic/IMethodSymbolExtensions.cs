#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
							 .Any(a => a.AttributeClass.InheritsFromOrEquals(attributeType));
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

		/// <summary>
		/// Gets the topmost non-local method containing the local function declaration. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localMethod">The method that can be local function.</param>
		/// <returns>
		/// The non-local method containing the local function.
		/// </returns>
		public static IMethodSymbol? GetContainingNonLocalMethod(this IMethodSymbol localMethod) =>
			GetStaticOrNonLocalContainingMethod(localMethod, stopOnStaticMethod: false);

		/// <summary>
		/// Gets the topmost static or non-local method containing the <paramref name="localMethod"/>. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localMethod">The method that can be local function.</param>
		/// <returns>
		/// the topmost static or non-local method containing the <paramref name="localMethod"/>.
		/// </returns>
		public static IMethodSymbol? GetStaticOrNonLocalContainingMethod(this IMethodSymbol localMethod) =>
			GetStaticOrNonLocalContainingMethod(localMethod, stopOnStaticMethod: true);

		private static IMethodSymbol? GetStaticOrNonLocalContainingMethod(IMethodSymbol localMethod, bool stopOnStaticMethod)
		{
			localMethod.ThrowOnNull(nameof(localMethod));

			if (localMethod.MethodKind != MethodKind.LocalFunction)
				return localMethod;

			IMethodSymbol? current = localMethod;

			while (current != null && current.MethodKind == MethodKind.LocalFunction && (!stopOnStaticMethod || !localMethod.IsStatic))
				current = current.ContainingSymbol as IMethodSymbol;

			return current;
		}

		/// <summary>
		/// Gets all parameters available for local method including parameters from containing methods.
		/// </summary>
		/// <param name="localMethod">The method that can be a local function.</param>
		/// <returns>
		/// all parameters available for the local method from containing methods.
		/// </returns>
		public static ImmutableArray<IParameterSymbol> GetAllParametersAvailableForLocalMethod(this IMethodSymbol localMethod)
		{
			if (localMethod.CheckIfNull(nameof(localMethod)).MethodKind != MethodKind.LocalFunction)
				return localMethod.Parameters;

			ImmutableArray<IParameterSymbol>.Builder parametersBuilder;

			if (localMethod.Parameters.IsDefaultOrEmpty)
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>();
			else
			{
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>(initialCapacity: localMethod.Parameters.Length);
				parametersBuilder.AddRange(localMethod.Parameters);
			}

			if (localMethod.IsStatic)
				return parametersBuilder.ToImmutable();

			IMethodSymbol? current = localMethod;

			do
			{
				var containingMethod = current.ContainingSymbol as IMethodSymbol;

				// For a non static nested local function we can add parameters from its containing local function even if it is static
				// But we must stop after that and won't take parameters from the methods containing static local function
				if (containingMethod != null && !containingMethod.Parameters.IsDefaultOrEmpty)
				{
					var notReassignedParameters = containingMethod.Parameters.Where(p => !parametersBuilder.Contains(p));
					parametersBuilder.AddRange(notReassignedParameters);
				}

				current = containingMethod;
			}
			while (current?.MethodKind == MethodKind.LocalFunction && !current.IsStatic);	

			return parametersBuilder.ToImmutable();
		}
	}
}
