using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;
using PX.Analyzers.Vsix.Utilities;




namespace PX.Analyzers.Coloriser
{
    internal static class TypeSymbolColoriserUtils
    {
        // Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces,
        // dealing only with original types.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InheritsOrImplementsOrEquals(this ITypeSymbol type, string baseTypeName, 
                                                        bool includeInterfaces = true)
        {
            if (type == null)
                return false;

            IEnumerable<ITypeSymbol> baseTypes = includeInterfaces 
                ? type.GetBaseTypesAndThis().ConcatStructList(type.AllInterfaces)
                : type.GetBaseTypesAndThis();

            return baseTypes.Select(typeSymbol => typeSymbol.Name)
                            .Contains(baseTypeName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ImplementsInterface(this ITypeSymbol type, string interfaceName)
        {
            if (type == null)
                return false;

            foreach (var interfaceSymbol in type.AllInterfaces)
            {
                if (interfaceSymbol.Name == interfaceName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An ITypeSymbol extension method that gets <see cref="ColoredCodeType"/> from identifier type symbol.
        /// </summary>
        /// <param name="identifierType">The identifierType to act on.</param>
        /// <returns/>  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ColoredCodeType? GetColoringTypeFromIdentifier(this ITypeSymbol identifierType)
        {
            if (!identifierType.IsValidForColoring(checkForNotColoredTypes: false))
                return null;

            IEnumerable<ITypeSymbol> typeHierarchy = identifierType.GetBaseTypes()
                                                                   .ConcatStructList(identifierType.AllInterfaces);

            ColoredCodeType? resolvedColoredCodeType = null;

            foreach (ITypeSymbol typeOrInterface in typeHierarchy)
            {
                if (TypeNames.TypeNamesToColoredCodeTypesForIdentifier.TryGetValue(typeOrInterface.Name, out ColoredCodeType coloredCodeType))
                {
                    resolvedColoredCodeType = coloredCodeType;
                    break;
                }
            }
       
            return resolvedColoredCodeType.HasValue 
                ? ValidateColorCodeTypeAndSymbolName(resolvedColoredCodeType.Value, identifierType.Name)
                : null;
        }

        /// <summary>
        /// An ITypeSymbol extension method that gets <see cref="ColoredCodeType"/> from generic Name type symbol.
        /// </summary>
        /// <param name="genericName">The generic Name type symbol to act on.</param>
        /// <returns/>  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ColoredCodeType? GetColoringTypeFromGenericName(this ITypeSymbol genericName)
        {
            if (!genericName.IsValidForColoring())
                return null;

            IEnumerable<ITypeSymbol> typeHierarchy = genericName.GetBaseTypes()
                                                                .ConcatStructList(genericName.AllInterfaces);

            ColoredCodeType? resolvedColoredCodeType = null;

            foreach (ITypeSymbol typeOrInterface in typeHierarchy)
            {
                if (TypeNames.TypeNamesToColoredCodeTypesForGenericName.TryGetValue(typeOrInterface.Name, out ColoredCodeType coloredCodeType))
                {
                    resolvedColoredCodeType = coloredCodeType;
                    break;
                }
            }

            return resolvedColoredCodeType.HasValue
                ? ValidateColorCodeTypeAndSymbolName(resolvedColoredCodeType.Value, genericName.Name)
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColoredCodeType? ValidateColorCodeTypeAndSymbolName(ColoredCodeType coloredCodeType, string typeSymbolName)
        {
            bool isValid = true;

            switch (coloredCodeType)
            {
                case ColoredCodeType.BqlOperator:
                case ColoredCodeType.BqlCommand:
                    isValid = typeSymbolName != TypeNames.BqlCommand;
                    break;
                case ColoredCodeType.DacExtension:
                    isValid = typeSymbolName != TypeNames.PXCacheExtension && typeSymbolName != TypeNames.PXCacheExtensionGeneric;
                    break;
                case ColoredCodeType.BQLConstantPrefix:
                case ColoredCodeType.BQLConstantEnding:
                    isValid = typeSymbolName != TypeNames.Constant && typeSymbolName != TypeNames.ConstantGeneric;
                    break;
                case ColoredCodeType.PXGraph:
                    isValid = typeSymbolName != TypeNames.PXGraph && typeSymbolName != TypeNames.PXGraphGeneric;
                    break;
            }

            return isValid ? coloredCodeType : (ColoredCodeType?)null;
        }

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
                case TypeKind.Array:                                 
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
            if (!typeSymbol.IsValidForColoring() || string.Equals(typeSymbol.Name, TypeNames.BqlCommand))
                return false;

            List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
                                                        .Select(type => type.Name)
                                                        .ToList();

            return typeHierarchyNames.Contains(TypeNames.PXSelectBaseType) ||
                   typeHierarchyNames.Contains(TypeNames.BqlCommand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBqlParameter(this ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsValidForColoring())
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlParameter);
        }

        public static bool IsBqlOperator(this ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsValidForColoring())
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
        public static bool IsDacExtension(this ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsValidForColoring() || string.Equals(typeSymbol.Name, TypeNames.PXCacheExtension))
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
        public static bool IsPXGraph(this ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsValidForColoring() || string.Equals(typeSymbol.Name, TypeNames.PXGraph))
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
