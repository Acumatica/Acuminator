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
	}
}
