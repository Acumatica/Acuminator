using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Utilities;


namespace Acuminator.Utilities
{
    public static class ITypeSymbolExtensions
    {
        private const string DefaultParameterName = "p";
        private const string DefaultBuiltInParameterName = "v";
      
        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
        {
            var current = type;

            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
        {
            var current = type.BaseType;

            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
      
        public static IEnumerable<ITypeSymbol> GetContainingTypesAndThis(this ITypeSymbol type)
        {
            var current = type;

            while (current != null)
            {
                yield return current;
                current = current.ContainingType;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetContainingTypes(this ITypeSymbol type)
        {
            var current = type.ContainingType;

            while (current != null)
            {
                yield return current;
                current = current.ContainingType;
            }
        }
        
        /// <summary>
        /// Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces, dealing only with original types.
        /// </summary>
        /// <param name="type">The type to act on.</param>
        /// <param name="baseType">The base type.</param>
        /// <param name="includeInterfaces">True to include, false to exclude the interfaces.</param>
        /// <returns/>
        public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
        {
            type.ThrowOnNull(nameof(type));
            baseType.ThrowOnNull(nameof(baseType));
	       
	        var typeList = type.GetBaseTypesAndThis();

            if (includeInterfaces)
            {
	            typeList = typeList.Concat(type.AllInterfaces);
            }

            return typeList.Any(t => t.Equals(baseType));
        }

        /// <summary>
        ///  Determine if "type" inherits from "baseType", ignoring constructed types and interfaces, dealing only with original types.
        /// </summary>
        /// <param name="type">The type to act on.</param>
        /// <param name="baseType">The base type.</param>
        /// <returns/>    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]   
        public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType) =>
            type.GetBaseTypesAndThis().Any(t => t.Equals(baseType));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType) => 
            InheritsFromOrEqualsGeneric(type, baseType, false);
	   
	    public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
	    {
            type.ThrowOnNull(nameof(type));
            baseType.ThrowOnNull(nameof(baseType));

            var typeList = type.GetBaseTypesAndThis();

		    if (includeInterfaces)
			    typeList = typeList.Concat(type.AllInterfaces);

			return typeList.Select(t => t.OriginalDefinition)
			               .Any(t => t.Equals(baseType.OriginalDefinition));
	    }

        public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces = false)
        {
            type.ThrowOnNull(nameof(type));
            baseType.ThrowOnNull(nameof(baseType));

            var list = type.GetBaseTypes();

            if (includeInterfaces)
                list = list.Concat(type.AllInterfaces);

            return list.Any(t => t.Equals(baseType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ImplementsInterface(this ITypeSymbol type, ITypeSymbol interfaceType)
        {
            type.ThrowOnNull(nameof(type));
            interfaceType.ThrowOnNull(nameof(interfaceType));

            if (!interfaceType.IsAbstract)
                throw new ArgumentException("Invalid interface type", nameof(interfaceType));

            return type.AllInterfaces.Any(t => t.Equals(interfaceType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InheritsFrom(this ITypeSymbol symbol, string baseType)
        {
            symbol.ThrowOnNull(nameof(symbol));
            baseType.ThrowOnNullOrWhiteSpace(nameof(baseType));

            ITypeSymbol current = symbol;

            while (current != null)
            {
                if (current.Name == baseType)
                    return true;

                current = current.BaseType;
            }

            return false;
        }

		/// <summary>
		/// Gets the depth of inheritance between <paramref name="type"/> and its <paramref name="baseType"/>.
		/// If <paramref name="baseType"/> is not an ancestor of type returns <c>null</c>.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseType">The base type.</param>
		/// <returns>
		/// The inheritance depth.
		/// </returns>
		public static int? GetInheritanceDepth(this ITypeSymbol type, ITypeSymbol baseType)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(type));

			ITypeSymbol current = type;
			int depth = 0;

			while (current != null && !current.Equals(baseType))
			{
				current = current.BaseType;
				depth++;
			}

			return current != null ? depth : (int?)null;
		}
    }
}