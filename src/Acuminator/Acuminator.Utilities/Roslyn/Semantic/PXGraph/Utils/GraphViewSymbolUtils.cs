using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using DataViewDelegatesOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax Node, Microsoft.CodeAnalysis.IMethodSymbol Symbol)>>;

using ViewOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>>;

using ViewSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphViewSymbolUtils
	{
		/// <summary>
		/// A delegate type for an action which extracts info DTOs about graph views/view delegates from <paramref name="graphOrgraphExtension"/> 
		/// and adds them to the <paramref name="viewInfos"/> collection with account for views/view delegates declaration order.
		/// Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="viewInfos">The action infos.</param>
		/// <param name="graphOrgraphExtension">The graph orgraph extension.</param>
		/// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
		/// <returns/>
		private delegate int AddViewInfoWithOrderDelegate<T>(OverridableItemsCollection<T> viewInfos,
															 ITypeSymbol graphOrgraphExtension, int startingOrder);

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
		public static ViewOverridableCollection GetViewsWithSymbolsFromPXGraph(this ITypeSymbol graph, PXContext pxContext,
																			   bool includeViewsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
				return Enumerable.Empty<OverridableItem<(ISymbol, INamedTypeSymbol)>>();

			var viewsByName = new OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)>();
			var graphViews = GetViewsFromGraphImpl(graph, pxContext, includeViewsFromInheritanceChain);
			viewsByName.AddRangeWithDeclarationOrder(graphViews, startingOrder: 0,
													 keySelector: view => view.Symbol.Name);
			return viewsByName.Items;
		}

		/// <summary>
		/// Get all views from graph or graph extension and its base graphs and base graph extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="graphOrExtension">The graph Or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns/>
		public static ViewOverridableCollection GetViewsFromGraphOrGraphExtensionAndBaseGraph(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			graphOrExtension.ThrowOnNull(nameof(graphOrExtension));

			bool isGraph = graphOrExtension.IsPXGraph(pxContext);

			if (!isGraph && !graphOrExtension.IsPXGraphExtension(pxContext))
				return Enumerable.Empty<OverridableItem<(ISymbol, INamedTypeSymbol)>>();

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
		public static ViewOverridableCollection GetViewsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetViewInfoFromGraphExtension<(ISymbol, INamedTypeSymbol)>(graphExtension, pxContext, AddViewsFromGraph, AddViewsFromGraphExtension);


			int AddViewsFromGraph(OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)> views,
								  ITypeSymbol graph, int startingOrder)
			{
				var graphViews = graph.GetViewsFromGraphImpl(pxContext);
				return views.AddRangeWithDeclarationOrder(graphViews, startingOrder, keySelector: v => v.Symbol.Name);
			}

			int AddViewsFromGraphExtension(OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)> views,
											ITypeSymbol graphExt, int startingOrder)
			{
				var extentionsViews = GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExt, pxContext);
				return views.AddRangeWithDeclarationOrder(extentionsViews, startingOrder, keySelector: v => v.Symbol.Name);
			}
		}

		private static ViewSymbolWithTypeCollection GetViewsFromGraphImpl(this ITypeSymbol graph, PXContext pxContext,
																		  bool includeViewsFromInheritanceChain = true)
		{
			if (includeViewsFromInheritanceChain)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(baseGraph, pxContext));
			}
			else
				return GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graph, pxContext);
		}

		private static ViewSymbolWithTypeCollection GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(ITypeSymbol graphOrExtension,
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
		/// Get the view delegates symbols and syntax nodes from the graph.
		/// The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graph">The graph to act on</param>
		/// <param name="viewsByName">The views of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <param name="inheritance">If true includes view delegates from the graph inheritance chain</param>
		/// <returns></returns>
		public static DataViewDelegatesOverridableCollection GetViewDelegatesFromGraph(this ITypeSymbol graph,
																						   IDictionary<string, DataViewInfo> viewsByName,
																						   PXContext pxContext, CancellationToken cancellation,
																						   bool inheritance = true)
		{
			graph.ThrowOnNull(nameof(graph));
			viewsByName.ThrowOnNull(nameof(viewsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!graph.IsPXGraph(pxContext))
				return Enumerable.Empty<OverridableItem<(MethodDeclarationSyntax, IMethodSymbol)>>();

			var viewDelegatesByName = new OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();
			var graphViewDelegates = GetViewDelegatesFromGraphImpl(graph, viewsByName, pxContext, inheritance, cancellation);

			viewDelegatesByName.AddRangeWithDeclarationOrder(graphViewDelegates, startingOrder: 0, keySelector: viewDel => viewDel.Symbol.Name);
			return viewDelegatesByName.Items;
		}

		/// <summary>
		/// Get the view delegates symbols and syntax nodes from the graph extension.
		/// The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="viewsByName">The views of the graph extension with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns></returns>
		public static DataViewDelegatesOverridableCollection GetViewDelegatesFromGraphExtensionAndBaseGraph(
														this ITypeSymbol graphExtension, IDictionary<string, DataViewInfo> viewsByName,
														PXContext pxContext, CancellationToken cancellation)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			viewsByName.ThrowOnNull(nameof(viewsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetViewInfoFromGraphExtension<(MethodDeclarationSyntax, IMethodSymbol)>(
				graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

			int AddDelegatesFromGraph(OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> delegates,
									   ITypeSymbol graph, int startingOrder)
			{
				var graphViewDelegates = graph.GetViewDelegatesFromGraphImpl(viewsByName, pxContext, inheritance: true, cancellation);
				return delegates.AddRangeWithDeclarationOrder(graphViewDelegates,
							startingOrder, keySelector: viewDelegate => viewDelegate.Symbol.Name);
			}

			int AddDelegatesFromGraphExtension(OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> delegates,
												ITypeSymbol graphExt, int startingOrder)
			{
				var extensionViewDelegates = graphExt.GetViewDelegatesFromGraphOrGraphExtension(viewsByName, pxContext, cancellation);
				return delegates.AddRangeWithDeclarationOrder(extensionViewDelegates,
										startingOrder, keySelector: viewDelegate => viewDelegate.Symbol.Name);
			}
		}

		private static IEnumerable<OverridableItem<T>> GetViewInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
																						AddViewInfoWithOrderDelegate<T> addGraphViewInfo,
																						AddViewInfoWithOrderDelegate<T> addGraphExtensionViewInfo)
		{
			var empty = Enumerable.Empty<OverridableItem<T>>();

			if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
			{
				return empty;
			}

			var graphType = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graphType == null)
				return empty;

			var allExtensionsFromBaseToDerived = graphExtension.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending,
																									includeGraph: false);
			if (allExtensionsFromBaseToDerived.IsNullOrEmpty())
				return empty;

			var infoByView = new OverridableItemsCollection<T>();
			int declarationOrder = addGraphViewInfo(infoByView, graphType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addGraphExtensionViewInfo(infoByView, extension, declarationOrder);
			}

			return infoByView.Items;
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphImpl(
																	this ITypeSymbol graph, IDictionary<string, DataViewInfo> viewsByName,
																	PXContext pxContext, bool inheritance, CancellationToken cancellation)
		{
			if (inheritance)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetViewDelegatesFromGraphOrGraphExtension(baseGraph, viewsByName, pxContext, cancellation));
			}
			else
			{
				return GetViewDelegatesFromGraphOrGraphExtension(graph, viewsByName, pxContext, cancellation);
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphOrGraphExtension(
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
	}
}