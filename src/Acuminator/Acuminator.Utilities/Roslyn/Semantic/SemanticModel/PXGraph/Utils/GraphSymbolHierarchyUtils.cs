using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A graph symbol hierarchy utility methods.
	/// </summary>
	public static class GraphSymbolHierarchyUtils
	{
		/// <summary>
		/// Gets the graph type with its base types up to first met <see cref="PX.Data.PXGraph"/>.
		/// </summary>
		/// <param name="graphType">The graph type to act on.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetGraphWithBaseTypes(this ITypeSymbol graphType)
		{
			graphType.ThrowOnNull(nameof(graphType));
			return graphType.GetBaseTypesAndThis()
							.TakeWhile(type => !type.IsGraphBaseType());
		}

		/// <summary>
		/// Gets the extension type with its base types up to first met <see cref="PX.Data.PXGraphExtension"/>.
		/// </summary>
		/// <param name="extensionType">The extension type to act on.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetExtensionWithBaseTypes(this ITypeSymbol extensionType)
		{
			extensionType.ThrowOnNull(nameof(extensionType));
			return extensionType.GetBaseTypesAndThis()
								.TakeWhile(type => !type.IsGraphExtensionBaseType());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsGraphOrGraphExtensionBaseType(this ITypeSymbol type) =>
			type?.Name == TypeNames.PXGraph || type.Name == TypeNames.PXGraphExtension;
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGraphBaseType(this ITypeSymbol type) => type?.Name == TypeNames.PXGraph;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGraphExtensionBaseType(this ITypeSymbol type) => 
			type?.Name == TypeNames.PXGraphExtension;

		/// <summary>
		/// Gets the graph extension with base graph extensions from graph extension type.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="sortDirection">The sort direction. The <see cref="SortDirection.Descending"/> order is from the extension to its base extensions/graph.
		/// The <see cref="SortDirection.Ascending"/> order is from the graph/base extensions to the most derived one.</param>
		/// <param name="includeGraph">True to include, false to exclude the graph type.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetGraphExtensionWithBaseExtensions(this ITypeSymbol graphExtension, PXContext pxContext,
																				   SortDirection sortDirection, bool includeGraph)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension == null || !graphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return Enumerable.Empty<ITypeSymbol>();

			INamedTypeSymbol extensionBaseType = graphExtension.GetBaseTypesAndThis()
															   .FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
			if (extensionBaseType == null)
				return Enumerable.Empty<ITypeSymbol>();

			var graphType = extensionBaseType.TypeArguments.LastOrDefault();

			if (graphType == null || !graphType.IsPXGraph(pxContext))
				return Enumerable.Empty<ITypeSymbol>();

			return sortDirection == SortDirection.Ascending
				? GetExtensionInAscendingOrder(graphType, graphExtension, extensionBaseType, pxContext, includeGraph)
				: GetExtensionInDescendingOrder(graphType, graphExtension, extensionBaseType, pxContext, includeGraph);			
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInAscendingOrder(ITypeSymbol graphType, ITypeSymbol graphExtension, 
																			 INamedTypeSymbol extensionBaseType, PXContext pxContext, bool includeGraph)
		{
			int graphIndex = extensionBaseType.TypeArguments.Length - 1;
			var extensions = new List<ITypeSymbol>(capacity: extensionBaseType.TypeArguments.Length);

			if (includeGraph)
			{
				extensions.AddRange(graphType.GetGraphWithBaseTypes().Reverse());
			}

			for (int i = graphIndex - 1; i >= 0; i--)
			{
				var baseExtension = extensionBaseType.TypeArguments[i];

				if (!baseExtension.IsPXGraphExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.Add(baseExtension);		//According to Platform team we shouldn't consider case when the graph extensions chaining mixes with .Net inheritance
			}

			extensions.AddRange(graphExtension.GetExtensionWithBaseTypes().Reverse());
			return extensions.Distinct();
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInDescendingOrder(ITypeSymbol graphType, ITypeSymbol graphExtension, 
																			  INamedTypeSymbol extensionBaseType, PXContext pxContext, bool includeGraph)
		{
			int graphIndex = extensionBaseType.TypeArguments.Length - 1;
			var extensions = new List<ITypeSymbol>(capacity: extensionBaseType.TypeArguments.Length);
			extensions.AddRange(graphExtension.GetExtensionWithBaseTypes());

			for (int i = 0; i <= graphIndex - 1; i++)
			{
				var baseExtension = extensionBaseType.TypeArguments[i];

				if (!baseExtension.IsPXGraphExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.Add(baseExtension);		//According to Platform team we shouldn't consider case when the graph extensions chaining mixes with .Net inheritance
			}

			if (includeGraph)
			{
				extensions.AddRange(graphType.GetGraphWithBaseTypes());
			}

			return extensions.Distinct();
		}
	}
}