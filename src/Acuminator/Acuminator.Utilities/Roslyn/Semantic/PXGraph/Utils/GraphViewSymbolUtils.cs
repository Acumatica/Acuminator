using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ViewSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphViewSymbolUtils
	{
		private const int EstimatedNumberOfViewsInGraph = 16;
		private const int EstimatedNumberOfViewDelegatesInGraph = 8;

		/// <summary>
		/// A delegate type for an action which extracts info DTOs about graph views/view delegates from <paramref name="graphOrgraphExtension"/> 
		/// and adds them to the <paramref name="viewInfos"/> collection with account for views/view delegates declaration order.
		/// Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="TInfo">Generic type parameter representing overridable info type.</typeparam>
		/// <param name="viewInfos">The action infos.</param>
		/// <param name="graphOrgraphExtension">The graph orgraph extension.</param>
		/// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
		/// <returns/>
		private delegate int AddViewInfoWithOrderDelegate<TInfo>(OverridableItemsCollection<TInfo> viewInfos,
																 ITypeSymbol graphOrgraphExtension, int startingOrder)
		where TInfo : IOverridableItem<TInfo>;

		/// <summary>
		/// Returns true if the data view is a processing view
		/// </summary>
		/// <param name="view">The type symbol of a data view</param>
		/// <param name="pxContext">The context</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsProcessingView(this ITypeSymbol view, PXContext pxContext)
		{
			view.ThrowOnNull(nameof(view));
			pxContext.ThrowOnNull(nameof(pxContext));

			return view.InheritsFromOrEqualsGeneric(pxContext.PXProcessingBase.Type);
		}

		/// <summary>
		/// Gets all declared view symbols and types from the graph and its base graphs,
		/// if there is a graphs class hierarchy and <paramref name="includeViewsFromInheritanceChain"/> parameter is <c>true</c>.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeViewsFromInheritanceChain">(Optional) True to include, false to exclude the views from inheritance chain.</param>
		/// <returns/>
		public static OverridableItemsCollection<DataViewInfo> GetViewsWithSymbolsFromPXGraph(this ITypeSymbol graph, PXContext pxContext,
																							  bool includeViewsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
				return new OverridableItemsCollection<DataViewInfo>();

			var viewsByName = new OverridableItemsCollection<DataViewInfo>(capacity: EstimatedNumberOfViewsInGraph);
			var graphViews = GetRawViewsFromGraphImpl(graph, pxContext, includeViewsFromInheritanceChain);
			viewsByName.AddRangeWithDeclarationOrder(graphViews, startingOrder: 0,
													 (view, order) => new DataViewInfo(view.ViewSymbol, view.ViewType, pxContext, order));
			return viewsByName;
		}

		/// <summary>
		/// Get all views from graph or graph extension and its base graphs and base graph extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="graphOrExtension">The graph Or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns/>
		public static OverridableItemsCollection<DataViewInfo> GetViewsFromGraphOrGraphExtensionAndBaseGraph(this ITypeSymbol graphOrExtension,
																											 PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			graphOrExtension.ThrowOnNull(nameof(graphOrExtension));

			bool isGraph = graphOrExtension.IsPXGraph(pxContext);

			if (!isGraph && !graphOrExtension.IsPXGraphExtension(pxContext))
				return new OverridableItemsCollection<DataViewInfo>();

			return isGraph
				? graphOrExtension.GetViewsWithSymbolsFromPXGraph(pxContext)
				: graphOrExtension.GetViewsFromGraphExtensionAndBaseGraph(pxContext);
		}

		/// <summary>
		/// Get all view symbols and types from the graph extension and its base graph
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="pxContext">Context</param>
		/// <returns></returns>
		public static OverridableItemsCollection<DataViewInfo> GetViewsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetViewInfoFromGraphExtension<DataViewInfo>(graphExtension, pxContext, AddViewsFromGraph, AddViewsFromGraphExtension);


			int AddViewsFromGraph(OverridableItemsCollection<DataViewInfo> views, ITypeSymbol graph, int startingOrder)
			{
				var rawGraphViews = graph.GetRawViewsFromGraphImpl(pxContext);
				return views.AddRangeWithDeclarationOrder(rawGraphViews, startingOrder,
														  (view, order) => new DataViewInfo(view.ViewSymbol, view.ViewType, pxContext, order));
			}

			int AddViewsFromGraphExtension(OverridableItemsCollection<DataViewInfo> views, ITypeSymbol graphExt, int startingOrder)
			{
				var rawGraphExtentionsViews = GetRawViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExt, pxContext);
				return views.AddRangeWithDeclarationOrder(rawGraphExtentionsViews, startingOrder,
														  (view, order) => new DataViewInfo(view.ViewSymbol, view.ViewType, pxContext, order));
			}
		}

		private static ViewSymbolWithTypeCollection GetRawViewsFromGraphImpl(this ITypeSymbol graph, PXContext pxContext,
																			 bool includeViewsFromInheritanceChain = true)
		{
			if (includeViewsFromInheritanceChain)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetRawViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(baseGraph, pxContext));
			}
			else
				return GetRawViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graph, pxContext);
		}

		private static ViewSymbolWithTypeCollection GetRawViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(ITypeSymbol graphOrExtension,
																												PXContext pxContext)
		{
			foreach (ISymbol member in graphOrExtension.GetMembers())
			{
				if (!(member is IFieldSymbol field) || field.DeclaredAccessibility != Accessibility.Public)
					continue;

				if (!(field.Type is INamedTypeSymbol fieldType) || !fieldType.InheritsFrom(pxContext.PXSelectBase.Type))
					continue;

				yield return (field, fieldType);
			}
		}

		/// <summary>
		/// Get the view delegates symbols and syntax nodes from the graph. The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="viewsByName">The views of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="inheritance">(Optional) If true includes view delegates from the graph inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// The view delegates from graph.
		/// </returns>
		public static OverridableItemsCollection<DataViewDelegateInfo> GetViewDelegatesFromGraph(this ITypeSymbol graph,
																						   IDictionary<string, DataViewInfo> viewsByName,
																						   PXContext pxContext, bool inheritance = true, 
																						   CancellationToken cancellation = default)
		{
			graph.ThrowOnNull(nameof(graph));
			viewsByName.ThrowOnNull(nameof(viewsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!graph.IsPXGraph(pxContext))
				return new OverridableItemsCollection<DataViewDelegateInfo>();

			var viewDelegatesByName = new OverridableItemsCollection<DataViewDelegateInfo>(capacity: EstimatedNumberOfViewDelegatesInGraph);
			var graphViewDelegates = GetRawViewDelegatesFromGraphImpl(graph, viewsByName, pxContext, inheritance, cancellation);

			viewDelegatesByName.AddRangeWithDeclarationOrder(graphViewDelegates, startingOrder: 0, 
															 (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			return viewDelegatesByName;
		}

		/// <summary>
		/// Get the view delegates symbols and syntax nodes from the graph extension.
		/// The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="viewsByName">The views of the graph extension with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns/>
		public static OverridableItemsCollection<DataViewDelegateInfo> GetViewDelegatesFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension,
																							IDictionary<string, DataViewInfo> viewsByName,
																							PXContext pxContext, CancellationToken cancellation)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			viewsByName.ThrowOnNull(nameof(viewsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetViewInfoFromGraphExtension<DataViewDelegateInfo>(graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

			int AddDelegatesFromGraph(OverridableItemsCollection<DataViewDelegateInfo> delegates, ITypeSymbol graph, int startingOrder)
			{
				var graphViewDelegates = graph.GetRawViewDelegatesFromGraphImpl(viewsByName, pxContext, inheritance: true, cancellation);
				return delegates.AddRangeWithDeclarationOrder(graphViewDelegates, startingOrder, 
															  (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			}

			int AddDelegatesFromGraphExtension(OverridableItemsCollection<DataViewDelegateInfo> delegates, ITypeSymbol graphExt, int startingOrder)
			{
				var extensionViewDelegates = graphExt.GetRawViewDelegatesFromGraphOrGraphExtension(viewsByName, pxContext, cancellation);
				return delegates.AddRangeWithDeclarationOrder(extensionViewDelegates, startingOrder, 
															  (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			}
		}

		

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetRawViewDelegatesFromGraphImpl(
																	this ITypeSymbol graph, IDictionary<string, DataViewInfo> viewsByName,
																	PXContext pxContext, bool inheritance, CancellationToken cancellation)
		{
			if (inheritance)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetRawViewDelegatesFromGraphOrGraphExtension(baseGraph, viewsByName, pxContext, cancellation));
			}
			else
			{
				return GetRawViewDelegatesFromGraphOrGraphExtension(graph, viewsByName, pxContext, cancellation);
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetRawViewDelegatesFromGraphOrGraphExtension(
															this ITypeSymbol graphOrExtension, IDictionary<string, DataViewInfo> viewsByName,
															PXContext pxContext, CancellationToken cancellation)
		{
			IEnumerable<IMethodSymbol> delegates = from method in graphOrExtension.GetMembers().OfType<IMethodSymbol>()
												   where method.IsValidViewDelegate(pxContext) && viewsByName.ContainsKey(method.Name)
												   select method;

			foreach (IMethodSymbol d in delegates)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxReference reference = d.DeclaringSyntaxReferences.FirstOrDefault();

				if (reference?.GetSyntax(cancellation) is MethodDeclarationSyntax declaration)
				{
					yield return (declaration, d);
				}
			}
		}

		private static OverridableItemsCollection<TInfo> GetViewInfoFromGraphExtension<TInfo>(ITypeSymbol graphExtension, PXContext pxContext,
																						AddViewInfoWithOrderDelegate<TInfo> addGraphViewInfo,
																						AddViewInfoWithOrderDelegate<TInfo> addGraphExtensionViewInfo)
		where TInfo : IOverridableItem<TInfo>
		{
			if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
				return new OverridableItemsCollection<TInfo>();

			var graphType = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graphType == null)
				return new OverridableItemsCollection<TInfo>();

			var allExtensionsFromBaseToDerived = graphExtension.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending,
																									includeGraph: false);
			if (allExtensionsFromBaseToDerived.IsNullOrEmpty())
				return new OverridableItemsCollection<TInfo>();

			int estimatedCapacity = typeof(TInfo) == typeof(DataViewInfo)
				? EstimatedNumberOfViewsInGraph
				: EstimatedNumberOfViewDelegatesInGraph;

			var infoByView = new OverridableItemsCollection<TInfo>(capacity: estimatedCapacity);
			int declarationOrder = addGraphViewInfo(infoByView, graphType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addGraphExtensionViewInfo(infoByView, extension, declarationOrder);
			}

			return infoByView;
		}
	}
}