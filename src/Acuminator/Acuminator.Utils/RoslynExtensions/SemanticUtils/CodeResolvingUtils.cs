using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers;



namespace Acuminator.Utilities
{
	public static class CodeResolvingUtils
	{
		/// <summary>
		/// An ITypeSymbol extension method that gets <see cref="PXCodeType"/> from identifier type symbol.
		/// </summary>
		/// <param name="identifierType">The identifierType to act on.</param>
		/// <param name="skipValidation">(Optional) True to skip validation.</param>
		/// <param name="checkItself">(Optional) True to check the type itself.</param>
		/// <returns>
		/// The coloring type from identifier.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PXCodeType? GetColoringTypeFromIdentifier(this ITypeSymbol identifierType, bool skipValidation = false,
																	 bool checkItself = false)
		{
			if (!skipValidation && !identifierType.IsValidForColoring())
				return null;

			IEnumerable<ITypeSymbol> typeHierarchy = checkItself
				? identifierType.GetBaseTypesAndThis()
				: identifierType.GetBaseTypes();

			typeHierarchy = typeHierarchy.ConcatStructList(identifierType.AllInterfaces);
			PXCodeType? resolvedColoredCodeType = null;

			foreach (ITypeSymbol typeOrInterface in typeHierarchy)
			{
				if (TypeNames.TypeNamesToCodeTypesForIdentifier.TryGetValue(typeOrInterface.Name, out PXCodeType coloredCodeType))
				{
					resolvedColoredCodeType = coloredCodeType;
					break;
				}
			}

			return resolvedColoredCodeType;
		}

		/// <summary>
		/// An ITypeSymbol extension method that gets <see cref="PXCodeType"/> from generic Name type symbol.
		/// </summary>
		/// <param name="genericName">The generic Name type symbol to act on.</param>
		/// <param name="genericNode">The generic node.</param>
		/// <returns>
		/// The code type from generic name.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PXCodeType? GetCodeTypeFromGenericName(this ITypeSymbol genericName)
		{
			if (!genericName.IsValidForColoring())
				return null;

			IEnumerable<ITypeSymbol> typeHierarchy = genericName.GetBaseTypes()
																.ConcatStructList(genericName.AllInterfaces);
			PXCodeType? resolvedColoredCodeType = null;

			foreach (ITypeSymbol typeOrInterface in typeHierarchy)
			{
				if (TypeNames.TypeNamesToCodeTypesForGenericName.TryGetValue(typeOrInterface.Name, out PXCodeType coloredCodeType))
				{
					resolvedColoredCodeType = coloredCodeType;
					break;
				}
			}

			return resolvedColoredCodeType;
		}

		/// <summary>
		/// An <see cref="ITypeSymbol"/> extension method that query if <paramref name="typeSymbol"/> is valid for coloring.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <param name="checkForNotColoredTypes">(Optional) True to check <paramref name="typeSymbol"/> in a list of not colored types.
		/// By default is true to prevent coloring of base types and interfaces.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidForColoring(this ITypeSymbol typeSymbol, bool checkForNotColoredTypes = true)
		{
			if (typeSymbol == null)
				return false;

			if (checkForNotColoredTypes && typeSymbol.IsAbstract && TypeNames.NotColoredTypes.Contains(typeSymbol.Name))
				return false;

			switch (typeSymbol.TypeKind)
			{
				case TypeKind.Unknown:
				case TypeKind.Delegate:
				case TypeKind.Dynamic:
				case TypeKind.Enum:
				case TypeKind.Error:
				case TypeKind.Interface:
				case TypeKind.Module:
				case TypeKind.Pointer:
				case TypeKind.Submission:
					return false;
				default:
					return true;
			}
		}

		/// <summary>
		/// An ITypeSymbol extension method that query if 'typeSymbol' is bql command.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <param name="context">The context.</param>
		/// <returns>
		/// True if bql command, false if not.
		/// </returns>
		public static bool IsBqlCommand(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
														.Select(type => type.Name)
														.ToList();

			return typeHierarchyNames.Contains(TypeNames.PXSelectBaseType) ||
				   typeHierarchyNames.Contains(TypeNames.BqlCommand);
		}

		public static bool IsBqlCommand(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			if (pxContext == null)
				return typeSymbol.IsBqlCommand();

			List<ITypeSymbol> typeHierarchy = typeSymbol.GetBaseTypesAndThis().ToList();
			return typeHierarchy.Contains(pxContext.PXSelectBaseType) || typeHierarchy.Contains(pxContext.BQL.BqlCommand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlParameter(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlParameter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlParameter(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			if (pxContext == null)
				return typeSymbol.IsBqlOperator();

			return typeSymbol.ImplementsInterface(pxContext.BQL.IBqlParameter);
		}

		public static bool IsBqlOperator(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring(checkForNotColoredTypes: false))
				return false;

			foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
			{
				if (interfaceSymbol.Name == TypeNames.IBqlCreator || interfaceSymbol.Name == TypeNames.IBqlJoin)
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDAC(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.ImplementsInterface(TypeNames.IBqlTable);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacExtension(this ITypeSymbol typeSymbol, bool ruleOutBaseTypes = false)
		{
			if (!typeSymbol.IsValidForColoring() || (ruleOutBaseTypes && string.Equals(typeSymbol.Name, TypeNames.PXCacheExtension)))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXCacheExtension, includeInterfaces: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacField(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.ImplementsInterface(TypeNames.IBqlField);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBqlConstant(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring() || typeSymbol.Name.StartsWith(TypeNames.Constant))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.Constant, includeInterfaces: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPXGraph(this ITypeSymbol typeSymbol, bool ruleOutBaseTypes = false)
		{
			if (!typeSymbol.IsValidForColoring() || (ruleOutBaseTypes && string.Equals(typeSymbol.Name, TypeNames.PXGraph)))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXGraph, includeInterfaces: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPXAction(this ITypeSymbol typeSymbol)
		{
			if (!typeSymbol.IsValidForColoring())
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXAction, includeInterfaces: false);
		}

		public static bool IsCustomBqlCommand(this ITypeSymbol bqlTypeSymbol, PXContext context)
		{
			bqlTypeSymbol.ThrowOnNull(nameof(bqlTypeSymbol));
			context.ThrowOnNull(nameof(context));

			const int pxSelectBaseStandartDepth = 2;
			int? pxSelectBaseDepth = bqlTypeSymbol.GetInheritanceDepth(context.PXSelectBaseType);

			if (pxSelectBaseDepth > pxSelectBaseStandartDepth)
				return true;

			const int bqlCommandBaseStandartDepth = 2;
			int? bqlCommandDepth = bqlTypeSymbol.GetInheritanceDepth(context.BQL.BqlCommand);
			return bqlCommandDepth > bqlCommandBaseStandartDepth;
		}

		public static TextSpan? GetBqlOperatorOutliningTextSpan(this ITypeSymbol typeSymbol, GenericNameSyntax bqlOperatorNode)
		{
			List<string> typesAndInterfaces = typeSymbol.AllInterfaces
														.Select(type => type.Name)
														.ToList();

			if (typesAndInterfaces.Contains(TypeNames.IBqlComparison) ||
				typesAndInterfaces.Contains(TypeNames.IBqlFunction) ||
				typesAndInterfaces.Contains(TypeNames.IBqlSortColumn))
			{
				return null;
			}

			if (typesAndInterfaces.Contains(TypeNames.IBqlAggregate) ||
				typesAndInterfaces.Contains(TypeNames.IBqlOrderBy))
			{
				return bqlOperatorNode.TypeArgumentList.Span;
			}

			if (typesAndInterfaces.Contains(TypeNames.IBqlJoin))
			{
				TypeArgumentListSyntax bqlJoinTypeParamsListSyntax = bqlOperatorNode.TypeArgumentList;
				var bqlJoinTypeArgumentsList = bqlJoinTypeParamsListSyntax.Arguments;

				switch (bqlJoinTypeArgumentsList.Count)
				{
					case 0:
					case 1:
						return null;
					case 2:
						return bqlJoinTypeParamsListSyntax.Span;
					default:                                                                                                            //Has next Join => we emit extra outlining tag
						int length = bqlJoinTypeArgumentsList.GetSeparator(1).SpanStart - bqlJoinTypeParamsListSyntax.SpanStart;
						return new TextSpan(bqlJoinTypeParamsListSyntax.SpanStart, length);
				}
			}

			if (!typesAndInterfaces.Contains(TypeNames.IBqlPredicateChain) &&
				!typesAndInterfaces.Contains(TypeNames.IBqlOn))
			{
				return null;
			}

			TypeArgumentListSyntax typeParamsListSyntax = bqlOperatorNode.TypeArgumentList;
			var typeArgumentsList = typeParamsListSyntax.Arguments;

			switch (typeArgumentsList.Count)
			{
				case 0:
				case 1:
					return null;
				case 2:
					return typeParamsListSyntax.Span;
				default:
					int length = typeArgumentsList.GetSeparator(1).SpanStart - typeParamsListSyntax.SpanStart;
					return new TextSpan(typeParamsListSyntax.SpanStart, length);
			}
		}
	}
}
