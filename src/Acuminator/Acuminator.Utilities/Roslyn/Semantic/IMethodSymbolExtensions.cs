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
		/// <param name="localFunction">The method that can be local function.</param>
		/// <returns>
		/// The non-local method containing the local function.
		/// </returns>
		public static IMethodSymbol? GetContainingNonLocalMethod(this IMethodSymbol localFunction) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: false);

		/// <summary>
		/// Gets the topmost static or non-local method containing the <paramref name="localFunction"/>. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localFunction">The method that can be local function.</param>
		/// <returns>
		/// the topmost static or non-local method containing the <paramref name="localFunction"/>.
		/// </returns>
		public static IMethodSymbol? GetStaticOrNonLocalContainingMethod(this IMethodSymbol localFunction) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: true);

		private static IMethodSymbol? GetStaticOrNonLocalContainingMethod(IMethodSymbol localFunction, bool stopOnStaticMethod)
		{
			localFunction.ThrowOnNull(nameof(localFunction));

			if (localFunction.MethodKind != MethodKind.LocalFunction)
				return localFunction;

			IMethodSymbol? current = localFunction;

			while (current != null && current.MethodKind == MethodKind.LocalFunction && (!stopOnStaticMethod || !localFunction.IsStatic))
				current = current.ContainingSymbol as IMethodSymbol;

			return current;
		}

		/// <summary>
		/// Gets all parameters available for local function including parameters from containing methods.
		/// </summary>
		/// <param name="localFunction">The method that can be a local function.</param>
		/// <param name="includeOwnParameters">True to include, false to exclude <paramref name="localFunction"/>'s own parameters.</param>
		/// <returns>
		/// All parameters available for the local function including parameters from containing methods.
		/// </returns>
		public static ImmutableArray<IParameterSymbol> GetAllParametersAvailableForLocalFunction(this IMethodSymbol localFunction, bool includeOwnParameters)
		{
			if (localFunction.CheckIfNull(nameof(localFunction)).MethodKind != MethodKind.LocalFunction)
				return localFunction.Parameters;

			ImmutableArray<IParameterSymbol>.Builder parametersBuilder;

			if (localFunction.Parameters.IsDefaultOrEmpty || !includeOwnParameters)
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>();
			else
			{
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>(initialCapacity: localFunction.Parameters.Length);
				parametersBuilder.AddRange(localFunction.Parameters);
			}

			if (localFunction.IsStatic)
				return parametersBuilder.ToImmutable();

			IMethodSymbol? current = localFunction;

			do
			{
				var containingMethod = current.ContainingSymbol as IMethodSymbol;

				// For a non static nested local function we can add parameters from its containing local function even if it is static
				// But we must stop after that and won't take parameters from the methods containing static local function
				if (containingMethod != null && !containingMethod.Parameters.IsDefaultOrEmpty)
				{
					// If we do not include parameters from the local function then check if the outer parameters are redefined by the local function parameters.
					// Redefined parameters won't be available to the local function
					var notReassignedParameters = from parameter in containingMethod.Parameters
												  where !parametersBuilder.Contains(parameter) && 												
														(includeOwnParameters || !localFunction.Parameters.Any(localParameter => localParameter.Name == parameter.Name))
												  select parameter;
					parametersBuilder.AddRange(notReassignedParameters);
				}

				current = containingMethod;
			}
			while (current?.MethodKind == MethodKind.LocalFunction && !current.IsStatic);	

			return parametersBuilder.ToImmutable();
		}

		/// <summary>
		/// Check if  the parameter <paramref name="parameterName"/> from the non local method is redefined.
		/// </summary>
		/// <param name="localMethod">The method that can be local function.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>
		/// True if non local method parameter is redefined in a local method, false if not.
		/// </returns>
		public static bool IsNonLocalMethodParameterRedefined(this IMethodSymbol localMethod, string parameterName)
		{
			localMethod.ThrowOnNull(nameof(localMethod));

			if (parameterName.IsNullOrWhiteSpace() || localMethod.MethodKind != MethodKind.LocalFunction)
				return false;

			IMethodSymbol? current = localMethod;

			while (current?.MethodKind == MethodKind.LocalFunction)
			{
				if (current.IsStatic || (!current.Parameters.IsDefaultOrEmpty && current.Parameters.Any(p => p.Name == parameterName)))
				{
					return true;
				}

				current = current.ContainingSymbol as IMethodSymbol;
			}

			return false;
		}
	}
}
