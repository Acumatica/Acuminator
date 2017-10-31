using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Coloriser
{
    internal static class TypeSymbolColoriserUtils
    {
        // Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces,
        // dealing only with original types.
        public static bool InheritsOrImplementsOrEquals(this ITypeSymbol type, string baseTypeName, 
                                                        bool includeInterfaces = true)
        {
            IEnumerable<ITypeSymbol> baseTypes = includeInterfaces 
                ? type.GetBaseTypesAndThis().Concat(type.AllInterfaces)
                : type.GetBaseTypesAndThis();

            return baseTypes.Select(typeSymbol => typeSymbol.Name)
                            .Contains(baseTypeName);
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
            if (typeSymbol == null)
                return false;

            List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
                                                        .Select(type => type.Name)
                                                        .ToList();

            return typeHierarchyNames.Contains(TypeNames.PXSelectBaseType) ||
                   typeHierarchyNames.Contains(TypeNames.BqlCommand);
        }

        public static bool IsBqlParameter(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlParameter);
        }

        public static bool IsBqlOperator(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            //Simple implementation for now. More complex should check concrete operator types could be added later
            List<string> typeHierarchyNames = typeSymbol.GetBaseTypesAndThis()
                                                        .Select(type => type.Name)
                                                        .ToList();

            return typeHierarchyNames.Contains(TypeNames.IBqlCreator) ||
                   typeHierarchyNames.Contains(TypeNames.IBqlJoin);
        }

        public static bool IsDAC(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null || string.Equals(typeSymbol.Name, TypeNames.IBqlTable))
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlTable);
        }

        public static bool IsDacField(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null || string.Equals(typeSymbol.Name, TypeNames.IBqlField))
                return false;

            return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.IBqlField);
        }

		public static bool IsBqlConstant(this ITypeSymbol typeSymbol)
		{
			if (typeSymbol == null || string.Equals(typeSymbol.Name, TypeNames.Constant))
				return false;

			return typeSymbol.InheritsOrImplementsOrEquals(TypeNames.Constant);
		}
	}
}
