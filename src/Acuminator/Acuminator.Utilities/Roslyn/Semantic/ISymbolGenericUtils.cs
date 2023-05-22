#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// A generic utilities for <see cref="ISymbol"/>.
	/// </summary>
	public static class ISymbolGenericUtils
	{
		public static bool IsReadOnly(this ISymbol symbol) =>
			symbol.CheckIfNull(nameof(symbol)) switch
			{
				IFieldSymbol field       => field.IsReadOnly,
				IPropertySymbol property => property.IsReadOnly, 
				ITypeSymbol type         => type.IsReadOnly(),
				_                        => false
			};

		public static bool IsReadOnly(this ITypeSymbol typeSymbol)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			var readonlyProperty = typeSymbol.GetType().GetProperty("IsReadOnly");

			if (readonlyProperty != null && readonlyProperty.PropertyType == typeof(bool))
			{
				try
				{
					return (bool)readonlyProperty.GetValue(typeSymbol);
				}
				catch (Exception e)
				{ 
				}
			}

			var typeNode = typeSymbol.GetSyntax() as MemberDeclarationSyntax;
			return typeNode?.IsReadOnly() ?? false;
		}

		/// <summary>
		/// Check if <paramref name="symbol"/> is explicitly declared in the code.
		/// </summary>
		/// <param name="symbol">The symbol to check.</param>
		/// <returns>
		/// True if <paramref name="symbol"/> explicitly declared, false if not.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExplicitlyDeclared(this ISymbol symbol) =>
			!symbol.CheckIfNull(nameof(symbol)).IsImplicitlyDeclared && symbol.CanBeReferencedByName;

		public static bool IsDeclaredInType(this ISymbol symbol, ITypeSymbol? type)
		{
			symbol.ThrowOnNull(nameof(symbol));
		
			if (type == null || symbol.ContainingType == null)
				return false;

			return symbol.ContainingType == type || symbol.ContainingType == type.OriginalDefinition;
		}

		/// <summary>
		/// Check if the <paramref name="symbol"/> has an attribute of a given <paramref name="attributeType"/>.
		/// </summary>
		/// <param name="symbol">The property to act on.</param>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="checkOverrides">True to check method overrides.</param>
		/// <param name="checkForDerivedAttributes">(Optional) True to check for attributes derived from <paramref name="attributeType"/>.</param>
		/// <returns>
		/// True if <paramref name="symbol"/> has attribute of <paramref name="attributeType"/>, false if not.
		/// </returns>
		public static bool HasAttribute<TSymbol>(this TSymbol symbol, INamedTypeSymbol attributeType, bool checkOverrides,
												 bool checkForDerivedAttributes = true)
		where TSymbol : class, ISymbol
		{
			symbol.ThrowOnNull(nameof(symbol));
			attributeType.ThrowOnNull(nameof(attributeType));

			Func<TSymbol, bool> attributeCheck = checkForDerivedAttributes
				? HasDerivedAttribute
				: HasAttribute;

			if (attributeCheck(symbol))
				return true;

			if (checkOverrides && symbol.IsOverride)
			{
				var overrides = symbol.GetOverrides();
				return overrides.Any(attributeCheck);
			}

			return false;

			//-----------------------------------------------------------
			bool HasAttribute(TSymbol symbolToCheck) =>
				symbolToCheck.GetAttributes()
							 .Any(a => a.AttributeClass.Equals(attributeType));

			bool HasDerivedAttribute(TSymbol symbolToCheck) =>
				symbolToCheck.GetAttributes()
							 .Any(a => a.AttributeClass.InheritsFromOrEquals(attributeType));
		}

		/// <summary>
		/// Gets the <paramref name="symbol"/> and its overrides.
		/// </summary>
		/// <param name="symbol">The symbol to act on.</param>
		/// <returns>
		/// The <paramref name="symbol"/> and its overrides.
		/// </returns>
		public static IEnumerable<TSymbol> GetOverridesAndThis<TSymbol>(this TSymbol symbol)
		where TSymbol : class, ISymbol
		{
			if (symbol.CheckIfNull(nameof(symbol)).IsOverride)
				return GetOverridesImpl(symbol, includeThis: true);
			else
				return new[] { symbol };
		}

		/// <summary>
		/// Gets the overrides of <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">The symbol to act on.</param>
		/// <returns>
		/// The overrides of <paramref name="symbol"/>.
		/// </returns>
		public static IEnumerable<TSymbol> GetOverrides<TSymbol>(this TSymbol symbol)
		where TSymbol : class, ISymbol
		{
			if (symbol.CheckIfNull(nameof(symbol)).IsOverride)
				return GetOverridesImpl(symbol, includeThis: false);
			else
				return Enumerable.Empty<TSymbol>();
		}

		private static IEnumerable<TSymbol> GetOverridesImpl<TSymbol>(TSymbol symbol, bool includeThis)
		where TSymbol : class, ISymbol
		{
			TSymbol? current = includeThis ? symbol : symbol.OverriddenSymbol();

			while (current != null)
			{
				yield return current;
				current = current.OverriddenSymbol();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TSymbol? OverriddenSymbol<TSymbol>(this TSymbol symbol)
		where TSymbol : class, ISymbol
		{
			return symbol switch
			{
				IMethodSymbol method	 => method.OverriddenMethod as TSymbol,
				IPropertySymbol property => property.OverriddenProperty as TSymbol,
				IEventSymbol @event		 => @event.OverriddenEvent as TSymbol,
				_						 => null
			};
		}
	}
}