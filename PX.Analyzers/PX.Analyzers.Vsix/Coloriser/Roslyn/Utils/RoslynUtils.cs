using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers.Coloriser
{
    /// <summary>
    /// A roslyn utilities. An independent from PX.Analyzers set of helpers(due to VS requirement for a strong name for PX.Analyzers)
    /// </summary>
    internal static class RoslynUtils
    {
        /// <summary>
        /// An ITypeSymbol extension method that query if 'typeSymbol' is bql command.
        /// </summary>
        /// <param name="typeSymbol">The typeSymbol to act on.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// True if bql command, false if not.
        /// </returns>
        public static bool IsBqlCommand(this ITypeSymbol typeSymbol, PXAcumaticaContext context)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.InheritsFrom(context.PXSelectBase))
                return true;

            return false;
        }

        public static IEnumerable<ITypeSymbol> GetBaseTypes(this ITypeSymbol type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static bool InheritsFrom(
            this ITypeSymbol type, ITypeSymbol baseType)
        {
            return type.GetBaseTypes().Any(t => t.Equals(baseType));
        }

        public static bool InheritsFrom(this ITypeSymbol symbol, string baseType)
        {
            var current = symbol;
            while (current != null)
            {
                if (current.Name == baseType) return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
