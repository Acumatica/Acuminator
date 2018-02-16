using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;
using System.Runtime.CompilerServices;



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
                ? type.GetBaseTypesAndThis().Concat(type.AllInterfaces)
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
        /// An ITypeSymbol extension method that query if 'typeSymbol' is bql command.
        /// </summary>
        /// <param name="typeSymbol">The typeSymbol to act on.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// True if bql command, false if not.
        /// </returns>
        public static bool IsBqlCommand(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null || string.Equals(typeSymbol.Name, TypeNames.BqlCommand))
                return false;

            List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
                                                        .Select(type => type.Name)
                                                        .ToList();

            return typeHierarchyNames.Contains(TypeNames.PXSelectBaseType) ||
                   typeHierarchyNames.Contains(TypeNames.BqlCommand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBqlParameter(this ITypeSymbol typeSymbol) => typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlParameter);


        public static bool IsBqlOperator(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
            {
                if (interfaceSymbol.Name == TypeNames.IBqlCreator || interfaceSymbol.Name == TypeNames.IBqlJoin)
                    return true;
            }

            return false;
        }    

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDAC(this ITypeSymbol typeSymbol) => typeSymbol.ImplementsInterface(TypeNames.IBqlTable);
        
        public static bool IsDacExtension(this ITypeSymbol typeSymbol)
        {
            if (string.Equals(typeSymbol?.Name, TypeNames.PXCacheExtension))
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXCacheExtension, includeInterfaces: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDacField(this ITypeSymbol typeSymbol) => typeSymbol.ImplementsInterface(TypeNames.IBqlField);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBqlConstant(this ITypeSymbol typeSymbol)
		{
			if (typeSymbol?.Name == null || typeSymbol.Name.StartsWith(TypeNames.Constant))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.Constant, includeInterfaces: false);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPXGraph(this ITypeSymbol typeSymbol)
        {
            if (string.Equals(typeSymbol?.Name, TypeNames.PXGraph))
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.PXGraph, includeInterfaces: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPXAction(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol?.Name == null)
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

            if (typesAndInterfaces.Contains(TypeNames.IBqlJoin) ||
                typesAndInterfaces.Contains(TypeNames.IBqlAggregate) ||
                typesAndInterfaces.Contains(TypeNames.IBqlOrderBy))
            {
                return bqlOperatorNode.TypeArgumentList.Span;
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
