using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
    public static class DacSymbolsHierarchyUtils
	{
        public static IEnumerable<ITypeSymbol> GetDacExtensionsWithDac(this ITypeSymbol dacExtension, PXContext pxContext)
        {
            dacExtension.ThrowOnNull(nameof(dacExtension));
            pxContext.ThrowOnNull(nameof(pxContext));

            if (!dacExtension.IsDacExtension(pxContext))
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

		/// <summary>
		/// Gets the DAC type with its base types up to the <see cref="System.Object"/>.
		/// </summary>
		/// <param name="dacType">The DAC type to act on.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetDacWithBaseTypes(this ITypeSymbol dacType) =>
			dacType.CheckIfNull(nameof(dacType))
				   .GetBaseTypesAndThis()
				   .TakeWhile(type => !type.IsDacBaseType());

		/// <summary>
		/// Gets the DAC extension type with its base types up to first met <c>PX.Data.PXCacheExtension</c>.
		/// </summary>
		/// <param name="extensionType">The DAC extension type to act on.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetDacExtensionWithBaseTypes(this ITypeSymbol extensionType) =>
			extensionType.CheckIfNull(nameof(extensionType))
						 .GetBaseTypesAndThis()
						 .TakeWhile(type => !type.IsDacExtensionBaseType());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacBaseType(this ITypeSymbol type) => type?.SpecialType == SpecialType.System_Object;
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDacExtensionBaseType(this ITypeSymbol type) => 
			type?.Name == TypeNames.PXCacheExtension;

		/// <summary>
		/// Gets the DAC extension with base DAC extensions from DAC extension type.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="sortDirection">The sort direction. The <see cref="SortDirection.Descending"/> order is from the extension to its base extensions/graph.
		/// The <see cref="SortDirection.Ascending"/> order is from the DAC/base extensions to the most derived one.</param>
		/// <param name="includeDac">True to include, false to exclude the DAC type.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetDacExtensionWithBaseExtensions(this ITypeSymbol dacExtension, PXContext pxContext,
																				 SortDirection sortDirection, bool includeDac)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (dacExtension == null || !dacExtension.IsDacExtension(pxContext))
				return Enumerable.Empty<ITypeSymbol>();

			var extensionBaseType = dacExtension.GetBaseTypesAndThis()
												.FirstOrDefault(type => type.IsDacExtensionBaseType()) as INamedTypeSymbol;
			if (extensionBaseType == null)
				return Enumerable.Empty<ITypeSymbol>();

			var dacType = extensionBaseType.TypeArguments.LastOrDefault();

			if (dacType == null || !dacType.IsDAC(pxContext))
				return Enumerable.Empty<ITypeSymbol>();

			return sortDirection == SortDirection.Ascending
				? GetExtensionInAscendingOrder(dacType, dacExtension, extensionBaseType, pxContext, includeDac)
				: GetExtensionInDescendingOrder(dacType, dacExtension, extensionBaseType, pxContext, includeDac);
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInAscendingOrder(ITypeSymbol dacType, ITypeSymbol dacExtension,
																			 INamedTypeSymbol extensionBaseType, PXContext pxContext, bool includeDac)
		{
			int dacIndex = extensionBaseType.TypeArguments.Length - 1;
			var extensions = new List<ITypeSymbol>(capacity: extensionBaseType.TypeArguments.Length);

			if (includeDac)
			{
				extensions.AddRange(dacType.GetDacWithBaseTypes().Reverse());
			}

			for (int i = dacIndex - 1; i >= 0; i--)
			{
				var baseExtension = extensionBaseType.TypeArguments[i];

				if (!baseExtension.IsDacExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.Add(baseExtension);      //According to Platform team we shouldn't consider case when the extensions chaining mixes with .Net inheritance
			}

			extensions.AddRange(dacExtension.GetDacExtensionWithBaseTypes().Reverse());
			return extensions.Distinct();
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInDescendingOrder(ITypeSymbol dacType, ITypeSymbol dacExtension,
																			  INamedTypeSymbol extensionBaseType, PXContext pxContext, bool includeDac)
		{
			int dacIndex = extensionBaseType.TypeArguments.Length - 1;
			var extensions = new List<ITypeSymbol>(capacity: extensionBaseType.TypeArguments.Length);
			extensions.AddRange(dacExtension.GetDacExtensionWithBaseTypes());

			for (int i = 0; i <= dacIndex - 1; i++)
			{
				var baseExtension = extensionBaseType.TypeArguments[i];

				if (!baseExtension.IsDacExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.Add(baseExtension);      //According to Platform team we shouldn't consider case when the extensions chaining mixes with .Net inheritance
			}

			if (includeDac)
			{
				extensions.AddRange(dacType.GetDacWithBaseTypes());
			}

			return extensions.Distinct();
		}
	}
}
