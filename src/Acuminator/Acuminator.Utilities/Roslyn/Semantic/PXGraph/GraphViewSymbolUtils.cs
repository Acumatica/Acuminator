using ActionSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System;
using ViewSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public static class GraphViewSymbolUtils
    {
        /// <summary>
        /// Gets all declared views from the graph and its base graphs if there is a graphs class hierarchy and <paramref name="includeViewsFromInheritanceChain"/> parameter is <c>true</c>.
        /// </summary>
        /// <param name="graph">The graph to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeViewsFromInheritanceChain">(Optional) True to include, false to exclude the views from inheritance chain.</param>
        /// <returns/>
        public static IEnumerable<INamedTypeSymbol> GetViewsFromPXGraph(this ITypeSymbol graph, PXContext pxContext,
                                                                        bool includeViewsFromInheritanceChain = true)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
                return Enumerable.Empty<INamedTypeSymbol>();

            if (includeViewsFromInheritanceChain)
            {
                return graph.GetBaseTypesAndThis()
                            .TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
                            .Reverse()
                            .SelectMany(baseGraph => GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(baseGraph, pxContext));
            }
            else
                return GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(graph, pxContext);
        }

        /// <summary>
        /// Gets all declared views from graph extension and its base graph extensions if there is a class hierarchy and <paramref name="includeViewsFromInheritanceChain"/> parameter is <c>true</c>.
        /// Does not include views from extension's graph.
        /// </summary>
        /// <param name="graphExtension">The graph extension to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeViewsFromInheritanceChain">(Optional) True to include, false to exclude the views from inheritance chain.</param>
        /// <returns/>
        public static IEnumerable<INamedTypeSymbol> GetViewsFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext,
                                                                               bool includeViewsFromInheritanceChain = true)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            if (graphExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
                return Enumerable.Empty<INamedTypeSymbol>();

            if (includeViewsFromInheritanceChain)
            {
                return graphExtension.GetBaseTypesAndThis()
                                     .TakeWhile(baseGraphExt => !baseGraphExt.IsGraphExtensionBaseType())
                                     .Reverse()
                                     .SelectMany(baseGraphExt => GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(baseGraphExt, pxContext));
            }
            else
                return GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(graphExtension, pxContext);
        }

        /// <summary>
        /// Gets the views from graph extension and its base graph.
        /// </summary>
        /// <param name="graphExtension">The graph extension to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeViewsFromExtensionsInheritanceChain">(Optional) True to include, false to exclude the views from extensions
        /// 														 inheritance chain.</param>
        /// <param name="includeViewsFromGraphsInheritanceChain">(Optional) True to include, false to exclude the views from graphs inheritance
        /// 													 chain.</param>
        /// <returns/>
        public static IEnumerable<INamedTypeSymbol> GetViewsFromGraphExtensionAndItsBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext,
                                                                                              bool includeViewsFromExtensionsInheritanceChain = true,
                                                                                              bool includeViewsFromGraphsInheritanceChain = true)
        {
            var extensionViews = graphExtension.GetViewsFromGraphExtension(pxContext, includeViewsFromExtensionsInheritanceChain);
            ITypeSymbol graph = graphExtension.GetGraphFromGraphExtension(pxContext);

            if (graph == null)
                return extensionViews;

            var graphViews = graph.GetViewsFromPXGraph(pxContext, includeViewsFromGraphsInheritanceChain);
            return graphViews.Concat(extensionViews);
        }

        private static IEnumerable<INamedTypeSymbol> GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(ITypeSymbol graphOrExtension, PXContext pxContext)
        {
            foreach (ISymbol member in graphOrExtension.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol field
                    when field.Type is INamedTypeSymbol fieldType && fieldType.InheritsFrom(pxContext.PXSelectBaseType):
                        yield return fieldType;
                        continue;
                    case IPropertySymbol property
                    when property.Type is INamedTypeSymbol propertyType && propertyType.InheritsFrom(pxContext.PXSelectBaseType):
                        yield return propertyType;
                        continue;
                }
            }
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
        public static ViewSymbolWithTypeCollection GetViewsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
        {
            graphExtension.ThrowOnNull(nameof(graphExtension));
            pxContext.ThrowOnNull(nameof(pxContext));

            return GetViewInfoFromGraphExtension<(ISymbol, INamedTypeSymbol)>(graphExtension, pxContext, AddViewsFromGraph, AddViewsFromGraphExtension);

            void AddViewsFromGraph(Dictionary<string, (ISymbol, INamedTypeSymbol)> views, ITypeSymbol graph)
            {
                IEnumerable<(ISymbol, INamedTypeSymbol)> graphViews = graph.GetViewsWithSymbolsFromPXGraph(pxContext);

                foreach ((ISymbol, INamedTypeSymbol) v in graphViews)
                {
                    views.Add(v.Item1.Name, v);
                }
            }

            void AddViewsFromGraphExtension(Dictionary<string, (ISymbol, INamedTypeSymbol)> views, ITypeSymbol graphExt)
            {
                IEnumerable<(ISymbol, INamedTypeSymbol)> extViews = GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExt, pxContext);

                foreach ((ISymbol, INamedTypeSymbol) v in extViews)
                {
                    views[v.Item1.Name] = v;
                }
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

                if (!(field.Type is INamedTypeSymbol fieldType) || !fieldType.InheritsFrom(pxContext.PXSelectBaseType))
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
        public static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphExtensionAndBaseGraph(
            this ITypeSymbol graphExtension, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            graphExtension.ThrowOnNull(nameof(graphExtension));
            views.ThrowOnNull(nameof(views));
            pxContext.ThrowOnNull(nameof(pxContext));

            return GetViewInfoFromGraphExtension<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>(
                graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

            void AddDelegatesFromGraph(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graph)
            {
                IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> dels = graph.GetViewDelegatesFromGraph(views, pxContext, cancellation);

                foreach ((MethodDeclarationSyntax, IMethodSymbol) d in dels)
                {
                    delegates.Add(d.Item2.Name, d);
                }
            }

            void AddDelegatesFromGraphExtension(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graphExt)
            {
                IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> dels = graphExt.GetViewDelegatesFromGraphOrGraphExtension(views, pxContext, cancellation);

                foreach ((MethodDeclarationSyntax, IMethodSymbol) d in dels)
                {
                    delegates[d.Item2.Name] = d;
                }
            }
        }

        private static IEnumerable<T> GetViewInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
            Action<Dictionary<string, T>, ITypeSymbol> addGraphViewInfo,
            Action<Dictionary<string, T>, ITypeSymbol> addGraphExtensionViewInfo)
        {
            IEnumerable<T> empty = Enumerable.Empty<T>();

            if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
            {
                return empty;
            }

            INamedTypeSymbol baseType = graphExtension.BaseType;
            Dictionary<string, T> infoByView = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            ITypeSymbol graphType = baseType.TypeArguments[baseType.TypeArguments.Length - 1];

            if (!graphType.IsPXGraph(pxContext))
            {
                return empty;
            }

            addGraphViewInfo(infoByView, graphType);

            if (baseType.TypeArguments.Length >= 2)
            {
                for (int i = baseType.TypeArguments.Length - 2; i >= 0; i--)
                {
                    ITypeSymbol argType = baseType.TypeArguments[i];

                    if (!argType.IsPXGraphExtension(pxContext))
                    {
                        return empty;
                    }

                    addGraphExtensionViewInfo(infoByView, argType);
                }
            }

            addGraphExtensionViewInfo(infoByView, graphExtension);

            return infoByView.Values;
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
