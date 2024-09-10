#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
			symbol.CheckIfNull() switch
			{
				IFieldSymbol field 		 => field.IsReadOnly,
				IPropertySymbol property => property.IsReadOnly,
				ITypeSymbol type 		 => type.IsReadOnly(),
				_ 						 => false
			};

		public static bool IsReadOnly(this ITypeSymbol typeSymbol)
		{
			typeSymbol.ThrowOnNull();

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
			!symbol.CheckIfNull().IsImplicitlyDeclared && symbol.CanBeReferencedByName;

		public static bool IsDeclaredInType(this ISymbol symbol, ITypeSymbol? type)
		{
			symbol.ThrowOnNull();
		
			if (type == null || symbol.ContainingType == null)
				return false;

			return symbol.ContainingType.Equals(type, SymbolEqualityComparer.Default) || 
				   symbol.ContainingType.Equals(type.OriginalDefinition, SymbolEqualityComparer.Default);
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
			symbol.ThrowOnNull();
			attributeType.ThrowOnNull();

			Func<TSymbol, bool> attributeCheck = checkForDerivedAttributes
				? HasDerivedAttribute
				: HasAttribute;

			if (attributeCheck(symbol))
				return true;

			if (checkOverrides && symbol.IsOverride)
			{
				var overrides = symbol.GetOverridden();
				return overrides.Any(attributeCheck);
			}

			return false;

			//-----------------------------------------------------------
			bool HasAttribute(TSymbol symbolToCheck)
			{
				var attributes = symbolToCheck.GetAttributes();
				return attributes.IsDefaultOrEmpty
					? false
					: attributes.Any(a => a.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);
			}

			bool HasDerivedAttribute(TSymbol symbolToCheck)
			{
				var attributes = symbolToCheck.GetAttributes();
				return attributes.IsDefaultOrEmpty
					? false
					: attributes.Any(a => a.AttributeClass?.InheritsFromOrEquals(attributeType) ?? false);
			}
		}

		/// <summary>
		/// Gets the <paramref name="symbol"/> and its overriden symbols.
		/// </summary>
		/// <param name="symbol">The symbol to act on.</param>
		/// <returns>
		/// The <paramref name="symbol"/> and its overriden symbols.
		/// </returns>
		public static IEnumerable<TSymbol> GetOverriddenAndThis<TSymbol>(this TSymbol symbol)
		where TSymbol : class, ISymbol
		{
			if (symbol.CheckIfNull().IsOverride)
				return GetOverriddenImpl(symbol, includeThis: true);
			else
				return [symbol];
		}

		/// <summary>
		/// Gets the overriden symbols of <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">The symbol to act on.</param>
		/// <returns>
		/// The overriden symbols of <paramref name="symbol"/>.
		/// </returns>
		public static IEnumerable<TSymbol> GetOverridden<TSymbol>(this TSymbol symbol)
		where TSymbol : class, ISymbol
		{
			if (symbol.CheckIfNull().IsOverride)
				return GetOverriddenImpl(symbol, includeThis: false);
			else
				return [];
		}

		private static IEnumerable<TSymbol> GetOverriddenImpl<TSymbol>(TSymbol symbol, bool includeThis)
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

		/// <summary>
		/// Query if <paramref name="symbol"/> is declared in source code.
		/// </summary>
		/// <param name="symbol">The symbol to check.</param>
		/// <returns>
		/// True if <paramref name="symbol"/> is in source code, false if not.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInSourceCode(this ISymbol symbol) =>
			!symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty;

		/// <summary>
		/// A <see cref="Location"/> extension method that returns <see langword="null"/> if location's <see cref="Location.Kind"/> is <see cref="LocationKind.None"/>.
		/// </summary>
		/// <param name="location">The location to act on.</param>
		/// <returns>
		/// The location or <see langword="null"/> if location's kind is <see cref="LocationKind.None"/>.
		/// </returns>
		/// <remarks>
		/// This method is a safety wrapper for locations that are obtained from <see cref="SyntaxToken.GetLocation"/> tokens.<br/>
		/// Syntax tokens may return a special null-object location with <see cref="LocationKind.None"/> kind.<br/>
		/// Such "null-object" location will prevent the usage of any fallback location that could have been obtained from coalesce chainings if the location was null.<br/>
		/// This helper allows to use coalsesce chaining with fallback locations in a safe manner.<br/><br/>
		/// Note, that locations obtained from <see cref="CSharpSyntaxNode.GetLocation"/> do not need to be checked this way.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Location? NullIfLocationKindIsNone(this Location? location) =>
			location?.Kind == LocationKind.None
				? null
				: location;
	}
}