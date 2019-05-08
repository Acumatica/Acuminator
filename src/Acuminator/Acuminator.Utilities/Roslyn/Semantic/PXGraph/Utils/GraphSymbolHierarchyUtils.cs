using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

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
		internal static bool IsGraphOrGraphExtensionBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type?.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraph ||
				   typeNameWithoutGenericArgsCount == TypeNames.PXGraphExtension;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGraphBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type?.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraph;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGraphExtensionBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type?.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraphExtension;
		}

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

			ImmutableArray<ITypeSymbol> typeArguments = extensionBaseType.TypeArguments;
			var (graphIndex, graphType) = GetGraphTypeWithIndexFromTypeArgs(typeArguments, pxContext);

			if (graphIndex < 0)
				return Enumerable.Empty<ITypeSymbol>();

			return sortDirection == SortDirection.Ascending
				? GetExtensionInAscendingOrder(typeArguments, graphIndex, graphType, graphExtension,pxContext, includeGraph)
				: GetExtensionInDescendingOrder(typeArguments, graphIndex, graphType, graphExtension, pxContext, includeGraph);			
		}

		private static (int GraphIndex, ITypeSymbol GraphType) GetGraphTypeWithIndexFromTypeArgs(in ImmutableArray<ITypeSymbol> typeArguments, 
																								 PXContext pxContext)
		{
			for (int i = typeArguments.Length - 1; i >= 0; i--)
			{
				ITypeSymbol typeArg = typeArguments[i];

				if (typeArg.IsPXGraph(pxContext))
					return (i, typeArg);
			}

			return (-1, null);
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInAscendingOrder(in ImmutableArray<ITypeSymbol> typeArguments, int graphIndex, 
																			 ITypeSymbol graphType, ITypeSymbol graphExtension, PXContext pxContext, 
																			 bool includeGraph)
		{
			var extensions = new List<ITypeSymbol>(capacity: graphIndex + 1);

			if (includeGraph)
			{
				extensions.AddRange(graphType.GetGraphWithBaseTypes().Reverse());
			}

			for (int i = graphIndex - 1; i >= 0; i--)
			{
				var baseExtension = typeArguments[i];

				if (!baseExtension.IsPXGraphExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.AddRange(baseExtension.GetExtensionWithBaseTypes().Reverse());
			}

			extensions.AddRange(graphExtension.GetExtensionWithBaseTypes().Reverse());
			return extensions.Distinct();
		}

		private static IEnumerable<ITypeSymbol> GetExtensionInDescendingOrder(in ImmutableArray<ITypeSymbol> typeArguments, int graphIndex,
																			  ITypeSymbol graphType, ITypeSymbol graphExtension, PXContext pxContext,
																			  bool includeGraph)
		{
			var extensions = new List<ITypeSymbol>(capacity: graphIndex + 1);
			extensions.AddRange(graphExtension.GetExtensionWithBaseTypes());

			for (int i = 0; i <= graphIndex - 1; i++)
			{
				var baseExtension = typeArguments[i];

				if (!baseExtension.IsPXGraphExtension(pxContext))
					return Enumerable.Empty<ITypeSymbol>();

				extensions.AddRange(baseExtension.GetExtensionWithBaseTypes());
			}

			if (includeGraph)
			{
				extensions.AddRange(graphType.GetGraphWithBaseTypes());
			}

			return extensions.Distinct();
		}
	}
}