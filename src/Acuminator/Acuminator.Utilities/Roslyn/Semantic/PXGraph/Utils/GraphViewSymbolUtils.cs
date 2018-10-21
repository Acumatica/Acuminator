using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using DataViewDelegatesOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax Node, Microsoft.CodeAnalysis.IMethodSymbol Symbol)>>;
using ViewOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>>;
using ViewSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>;
//using ViewByNameCollection = System.Collections.Generic.Dictionary<string, (Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.INamedTypeSymbol)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public static class GraphViewSymbolUtils
    {
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
        public static ViewSymbolWithTypeCollection GetViewsWithSymbolsFromPXGraph(this ITypeSymbol graph, PXContext pxContext,
                                                                                  bool includeViewsFromInheritanceChain = true)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
                return Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

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

            void AddViewsFromGraph(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> views, ITypeSymbol graph)
            {
                var graphViews = graph.GetViewsWithSymbolsFromPXGraph(pxContext);

                graphViews.ForEach(v => views.Add(v.ViewSymbol.Name, v));
            }

            void AddViewsFromGraphExtension(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> views, ITypeSymbol graphExt)
            {
                var extViews = GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExt, pxContext);

                extViews.ForEach(v => views.Add(v.ViewSymbol.Name, v));
            }
        }

        /// <summary>
        /// Gets all declared view symbols with view types from graph extension and its base graph extensions,
        /// if there is a class hierarchy and <paramref name="includeViewsFromInheritanceChain"/> parameter is <c>true</c>.
        /// Does not include views from extension's graph.
        /// </summary>
        /// <param name="graphExtension">The graph extension to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeViewsFromInheritanceChain">(Optional) True to include, false to exclude the views from inheritance chain.</param>
        /// <returns/>
        public static ViewSymbolWithTypeCollection GetViewSymbolsWithTypesFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext,
                                                                                             bool includeViewsFromInheritanceChain = true)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            if (graphExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
                return Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

            if (includeViewsFromInheritanceChain)
            {
                return graphExtension.GetBaseTypesAndThis()
                                     .TakeWhile(baseGraphExt => !baseGraphExt.IsGraphExtensionBaseType())
                                     .Reverse()
                                     .SelectMany(baseGraphExt => GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(baseGraphExt, pxContext));
            }
            else
                return GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExtension, pxContext);
        }

        /// <summary>
        /// Gets the view symbols with view types from graph extension and its base graph.
        /// </summary>
        /// <param name="graphExtension">The graph extension to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeViewsFromExtensionsInheritanceChain">(Optional) True to include, false to exclude the views from extensions
        /// 														 inheritance chain.</param>
        /// <param name="includeViewsFromGraphsInheritanceChain">(Optional) True to include, false to exclude the views from graphs inheritance
        /// 													 chain.</param>
        /// <returns/>
        public static ViewSymbolWithTypeCollection GetViewSymbolsWithTypesFromGraphExtensionAndItsBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext,
                                                                                                            bool includeViewsFromExtensionsInheritanceChain = true,
                                                                                                            bool includeViewsFromGraphsInheritanceChain = true)
        {
            var extensionViewSymbolWithTypes = graphExtension.GetViewSymbolsWithTypesFromGraphExtension(pxContext, includeViewsFromExtensionsInheritanceChain);
            ITypeSymbol graph = graphExtension.GetGraphFromGraphExtension(pxContext);

            if (graph == null)
                return extensionViewSymbolWithTypes;

            var graphViewsSymbolWithTypes = graph.GetViewsWithSymbolsFromPXGraph(pxContext, includeViewsFromGraphsInheritanceChain);
            return graphViewsSymbolWithTypes.Concat(extensionViewSymbolWithTypes);
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
        /// Get the view delegates symbols and syntax nodes from the graph
        /// </summary>
        /// <param name="graph">The graph to act on</param>
        /// <param name="views">The views of the graph</param>
        /// <param name="pxContext">Context</param>
        /// <param name="cancellation">Cancellation token</param>
        /// <param name="inheritance">If true includes view delegates from the graph inheritance chain</param>
        /// <returns></returns>
        public static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraph(
            this ITypeSymbol graph, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation, bool inheritance = true)
        {
            graph.ThrowOnNull(nameof(graph));
            views.ThrowOnNull(nameof(views));
            pxContext.ThrowOnNull(nameof(pxContext));

            if (!graph.IsPXGraph(pxContext))
            {
                return Enumerable.Empty<(MethodDeclarationSyntax, IMethodSymbol)>();
            }

            if (inheritance)
            {
                return graph.GetBaseTypesAndThis()
                            .TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
                            .SelectMany(baseGraph => GetViewDelegatesFromGraphOrGraphExtension(baseGraph, views, pxContext, cancellation));
            }
            else
            {
                return GetViewDelegatesFromGraphOrGraphExtension(graph, views, pxContext, cancellation);
            }
        }

        /// <summary>
        /// Get the view delegates symbols and syntax nodes from the graph extension
        /// </summary>
        /// <param name="graphExtension">The graph extension to act on</param>
        /// <param name="views">The views of the graph extension</param>
        /// <param name="pxContext">Context</param>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns></returns>
        public static DataViewDelegatesOverridableCollection GetViewDelegatesFromGraphExtensionAndBaseGraph(
            this ITypeSymbol graphExtension, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            graphExtension.ThrowOnNull(nameof(graphExtension));
            views.ThrowOnNull(nameof(views));
            pxContext.ThrowOnNull(nameof(pxContext));

            return GetViewInfoFromGraphExtension<(MethodDeclarationSyntax, IMethodSymbol)>(
                graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

            void AddDelegatesFromGraph(GraphOverridableItemsCollection<(MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graph)
            {
                var dels = graph.GetViewDelegatesFromGraph(views, pxContext, cancellation);

                dels.ForEach(d => delegates.Add(d.Symbol.Name, d));
            }

            void AddDelegatesFromGraphExtension(GraphOverridableItemsCollection<(MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graphExt)
            {
                var dels = graphExt.GetViewDelegatesFromGraphOrGraphExtension(views, pxContext, cancellation);

                dels.ForEach(d => delegates.Add(d.Symbol.Name, d));
            }
        }

        private static IEnumerable<GraphOverridableItem<T>> GetViewInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
            Action<GraphOverridableItemsCollection<T>, ITypeSymbol> addGraphViewInfo,
            Action<GraphOverridableItemsCollection<T>, ITypeSymbol> addGraphExtensionViewInfo)
        {
            var empty = Enumerable.Empty<GraphOverridableItem<T>>();

            if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
            {
                return empty;
            }

            var infoByView = new GraphOverridableItemsCollection<T>();
            var baseType = graphExtension.BaseType;
            var graphType = baseType.TypeArguments[baseType.TypeArguments.Length - 1];

            if (!graphType.IsPXGraph(pxContext))
            {
                return empty;
            }

            addGraphViewInfo(infoByView, graphType);

            if (baseType.TypeArguments.Length >= 2)
            {
                for (int i = baseType.TypeArguments.Length - 2; i >= 0; i--)
                {
                    var argType = baseType.TypeArguments[i];

                    if (!argType.IsPXGraphExtension(pxContext))
                    {
                        return empty;
                    }

                    addGraphExtensionViewInfo(infoByView, argType);
                }
            }

            addGraphExtensionViewInfo(infoByView, graphExtension);

            return infoByView.Items;
        }

        private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphOrGraphExtension(
            this ITypeSymbol graphOrExtension, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            IEnumerable<IMethodSymbol> delegates = graphOrExtension.GetMethods()
                                                   .Where(m => views.Any(v => v.Name.Equals(m.Name, StringComparison.OrdinalIgnoreCase)))
                                                   .Where(m => m.ReturnType.Equals(pxContext.SystemTypes.IEnumerable))
                                                   .Where(m => m.Parameters.All(p => p.RefKind != RefKind.Ref));
            foreach (IMethodSymbol d in delegates)
            {
                cancellation.ThrowIfCancellationRequested();

                SyntaxReference reference = d.DeclaringSyntaxReferences.FirstOrDefault();

                if (!(reference?.GetSyntax(cancellation) is MethodDeclarationSyntax declaration))
                {
                    continue;
                }

                yield return (declaration, d);
            }
        }
    }
}
