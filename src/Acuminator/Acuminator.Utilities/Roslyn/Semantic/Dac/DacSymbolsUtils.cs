using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
    public static class DacSymbolsUtils
    {
        public static IEnumerable<ITypeSymbol> GetDacExtensionsWithDac(this ITypeSymbol dacExtension, PXContext pxContext)
        {
            dacExtension.ThrowOnNull(nameof(dacExtension));
            pxContext.ThrowOnNull(nameof(pxContext));

            if (!dacExtension.InheritsFrom(pxContext.PXCacheExtensionType))
            {
                return Enumerable.Empty<ITypeSymbol>();
            }

            var extensionBaseType = dacExtension.BaseType;
            var typeArguments = extensionBaseType.TypeArguments;
            var dacType = typeArguments.LastOrDefault();

            if (dacType == null || !dacType.IsDAC(pxContext))
            {
                return Enumerable.Empty<ITypeSymbol>();
            }

            var types = new List<ITypeSymbol>(typeArguments.Length + 1) { dacExtension };
            var typeArgumentsExceptDac = typeArguments.Take(typeArguments.Length - 1);

            foreach (var ta in typeArgumentsExceptDac)
            {
                if (!ta.IsDacExtension(pxContext))
                {
                    return Enumerable.Empty<ITypeSymbol>();
                }

                types.Add(ta);
            }

            types.Add(dacType);

            return types;
        }
    }
}
