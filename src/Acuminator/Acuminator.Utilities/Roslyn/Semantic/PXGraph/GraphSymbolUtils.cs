using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using ViewSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>;
using ActionSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphSymbolUtils
	{
		/// <summary>
		/// Gets the graph type from graph extension type.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The graph from graph extension.
		/// </returns>
		public static ITypeSymbol GetGraphFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension == null || !graphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return null;

			var baseGraphExtensionType = graphExtension.GetBaseTypesAndThis()
													   .OfType<INamedTypeSymbol>()
													   .FirstOrDefault(type => IsGraphExtensionBaseType(type));
			if (baseGraphExtensionType == null)
				return null;

			var graphExtTypeArgs = baseGraphExtensionType.TypeArguments;

			if (graphExtTypeArgs.Length == 0)
				return null;

			ITypeSymbol firstTypeArg = graphExtTypeArgs[0];

			if (!(firstTypeArg is ITypeSymbol pxGraph) || !pxGraph.IsPXGraph())
				return null;

			return pxGraph;
		}

		#region View Types
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

			if (graph?.InheritsFrom(pxContext.PXGraphType) != true)
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
        #endregion

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

        public static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphExtensionAndBaseGraph(
            this ITypeSymbol graphExtension, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            graphExtension.ThrowOnNull(nameof(graphExtension));
            views.ThrowOnNull(nameof(views));
            pxContext.ThrowOnNull(nameof(pxContext));

            return GetViewInfoFromGraphExtension<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>(
                graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

            void AddDelegatesFromGraph(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> d, ITypeSymbol t, PXContext c) =>
                AddGraphViewDelegates(d, t, views, c, cancellation);
            void AddDelegatesFromGraphExtension(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> d, ITypeSymbol t, PXContext c) =>
                AddGraphExtensionViewDelegates(d, t, views, c, cancellation);

            /*IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> empty = Enumerable.Empty<(MethodDeclarationSyntax, IMethodSymbol)>();

            if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
            {
                return empty;
            }

            INamedTypeSymbol baseType = graphExtension.BaseType;
            Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> delegates = new Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)>(
                StringComparer.OrdinalIgnoreCase);

            for (int i = baseType.TypeArguments.Length - 1; i >= 0; i--)
            {
                ITypeSymbol argType = baseType.TypeArguments[i];

                if (i == baseType.TypeArguments.Length - 1)
                {
                    if (!argType.IsPXGraph(pxContext))
                    {
                        return empty;
                    }

                    AddGraphViewDelegates(delegates, argType, views, pxContext, cancellation);
                }
                else
                {
                    if (!argType.IsPXGraphExtension(pxContext))
                    {
                        return empty;
                    }

                    AddGraphExtensionViewDelegates(delegates, argType, views, pxContext, cancellation);
                }
            }

            AddGraphExtensionViewDelegates(delegates, graphExtension, views, pxContext, cancellation);

            return delegates.Values;*/
        }

        private static IEnumerable<T> GetViewInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
            Action<Dictionary<string, T>, ITypeSymbol, PXContext> addGraphViewInfo,
            Action<Dictionary<string, T>, ITypeSymbol, PXContext> addGraphExtensionViewInfo)
        {
            IEnumerable<T> empty = Enumerable.Empty<T>();

            if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
            {
                return empty;
            }

            INamedTypeSymbol baseType = graphExtension.BaseType;
            Dictionary<string, T> infoByView = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            for (int i = baseType.TypeArguments.Length - 1; i >= 0; i--)
            {
                ITypeSymbol argType = baseType.TypeArguments[i];

                if (i == baseType.TypeArguments.Length - 1)
                {
                    if (!argType.IsPXGraph(pxContext))
                    {
                        return empty;
                    }

                    addGraphViewInfo(infoByView, argType, pxContext);
                }
                else
                {
                    if (!argType.IsPXGraphExtension(pxContext))
                    {
                        return empty;
                    }

                    addGraphExtensionViewInfo(infoByView, argType, pxContext);
                }
            }

            addGraphExtensionViewInfo(infoByView, graphExtension, pxContext);

            return infoByView.Values;
        }

        private static void AddGraphViewDelegates(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graph,
            IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> dels = graph.GetViewDelegatesFromGraph(views, pxContext, cancellation);

            foreach ((MethodDeclarationSyntax, IMethodSymbol) d in dels)
            {
                delegates.Add(d.Item2.Name, d);
            }
        }

        private static void AddGraphExtensionViewDelegates(Dictionary<string, (MethodDeclarationSyntax, IMethodSymbol)> delegates, ITypeSymbol graph,
            IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> dels = graph.GetViewDelegatesFromGraphOrGraphExtension(views, pxContext, cancellation);

            foreach ((MethodDeclarationSyntax, IMethodSymbol) d in dels)
            {
                delegates[d.Item2.Name] = d;
            }
        }

        private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetViewDelegatesFromGraphOrGraphExtension(
            this ITypeSymbol graphOrExtension, IEnumerable<ISymbol> views, PXContext pxContext, CancellationToken cancellation)
        {
            foreach (ISymbol member in graphOrExtension.GetMembers())
            {
                cancellation.ThrowIfCancellationRequested();

                if (!(member is IMethodSymbol method))
                {
                    continue;
                }

                bool correctName = views.Any(v => v.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase));
                if (!correctName)
                {
                    continue;
                }

                bool correctReturnType = pxContext.SystemTypes.IEnumerable.Equals(method.ReturnType);
                if (!correctReturnType)
                {
                    continue;
                }

                bool hasReferencePar = method.Parameters.Any(p => p.Type.IsReferenceType);
                if (hasReferencePar)
                {
                    continue;
                }

                MethodDeclarationSyntax node = method.DeclaringSyntaxReferences.First().GetSyntax(cancellation) as MethodDeclarationSyntax;

                yield return (node, method);
            }
        }

        #region View Symbol And Types
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

			if (graph?.InheritsFrom(pxContext.PXGraphType) != true)
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

        public static ViewSymbolWithTypeCollection GetViewsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
        {
            graphExtension.ThrowOnNull(nameof(graphExtension));
            pxContext.ThrowOnNull(nameof(pxContext));

            return GetViewInfoFromGraphExtension<(ISymbol, INamedTypeSymbol)>(graphExtension, pxContext, AddViewsFromGraph, AddViewsFromGraphExtension);

            void AddViewsFromGraph(Dictionary<string, (ISymbol, INamedTypeSymbol)> v, ITypeSymbol t, PXContext c) =>
                AddGraphViews(v, t, c);
            void AddViewsFromGraphExtension(Dictionary<string, (ISymbol, INamedTypeSymbol)> v, ITypeSymbol t, PXContext c) =>
                AddGraphViews(v, t, c);

            /*ViewSymbolWithTypeCollection empty = Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

            if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
            {
                return empty;
            }

            INamedTypeSymbol baseType = graphExtension.BaseType;
            Dictionary<string, (ISymbol, INamedTypeSymbol)> views = new Dictionary<string, (ISymbol, INamedTypeSymbol)>();

            for (int i = baseType.TypeArguments.Length - 1; i >= 0; i--)
            {
                ITypeSymbol argType = baseType.TypeArguments[i];

                if (i == baseType.TypeArguments.Length - 1)
                {
                    if (!argType.IsPXGraph(pxContext))
                    {
                        return empty;
                    }

                    AddGraphViews(views, argType, pxContext);
                }
                else
                {
                    if (!argType.IsPXGraphExtension(pxContext))
                    {
                        return empty;
                    }

                    AddGraphExtensionViews(views, argType, pxContext);
                }
            }

            AddGraphExtensionViews(views, graphExtension, pxContext);

            return views.Values;*/
        }

        private static void AddGraphExtensionViews(Dictionary<string, (ISymbol, INamedTypeSymbol)> views, ITypeSymbol graphExtType, PXContext pxContext)
        {
            IEnumerable<(ISymbol, INamedTypeSymbol)> extViews = GetAllViewSymbolsWithTypesFromPXGraphOrPXGraphExtensionImpl(graphExtType, pxContext);

            foreach ((ISymbol, INamedTypeSymbol) v in extViews)
            {
                views[v.Item1.Name] = v;
            }
        }

        private static void AddGraphViews(Dictionary<string, (ISymbol, INamedTypeSymbol)> views, ITypeSymbol graphType, PXContext pxContext)
        {
            IEnumerable<(ISymbol, INamedTypeSymbol)> graphViews = graphType.GetViewsWithSymbolsFromPXGraph(pxContext);

            foreach ((ISymbol, INamedTypeSymbol) v in graphViews)
            {
                views.Add(v.Item1.Name, v);
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
		#endregion

		#region PXAction types
		/// <summary>
		/// Gets the PXActions from graph and, if <paramref name="includeActionsFromInheritanceChain"/> is <c>true</c>, its base graphs.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromInheritanceChain">(Optional) True to include, false to exclude the actions from the inheritance chain.</param>
		/// <returns/>
		public static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraph(this ITypeSymbol graph, PXContext pxContext,
																		  bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph?.InheritsFrom(pxContext.PXGraphType) != true)
				return Enumerable.Empty<INamedTypeSymbol>();

			if (includeActionsFromInheritanceChain)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => baseGraph.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
				return graph.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext);
		}

		/// <summary>
		/// Gets all declared actions from graph extension and its base graph extensions if there is a class hierarchy and <paramref name="includeActionsFromInheritanceChain"/> parameter is <c>true</c>.
		/// Does not include actions from extension's graph.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromInheritanceChain">(Optional) True to include, false to exclude the actions from inheritance chain.</param>
		/// <returns/>
		public static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext,
																				   bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
				return Enumerable.Empty<INamedTypeSymbol>();

			if (includeActionsFromInheritanceChain)
			{
				return graphExtension.GetBaseTypesAndThis()
									 .TakeWhile(baseGraphExt => !baseGraphExt.IsGraphExtensionBaseType())
									 .Reverse()
									 .SelectMany(baseGraphExt => baseGraphExt.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
				return graphExtension.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext);
		}

		/// <summary>
		/// Gets the PXActions declared on the graph extension and its base graph.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromExtensionInheritanceChain">(Optional) True to include, false to exclude the actions from extension inheritance chain.</param>
		/// <param name="includeActionsFromGraphInheritanceChain">(Optional) True to include, false to exclude the actions from graph inheritance chain.</param>
		/// <returns/>
		public static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraphExtensionAndItsBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext,
																								  bool includeActionsFromExtensionInheritanceChain = true,
																								  bool includeActionsFromGraphInheritanceChain = true)
		{
			var extensionActions = graphExtension.GetPXActionsFromGraphExtension(pxContext, includeActionsFromExtensionInheritanceChain);
			ITypeSymbol graph = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graph == null)
				return extensionActions;

			var graphActions = graph.GetPXActionsFromGraph(pxContext, includeActionsFromGraphInheritanceChain);
			return graphActions.Concat(extensionActions);
		}

		private static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraphOrGraphExtensionImpl(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			foreach (ISymbol member in graphOrExtension.GetMembers())
			{
				switch (member)
				{
					case IFieldSymbol field when field.Type is INamedTypeSymbol fieldType && fieldType.IsPXAction():
						yield return fieldType;
						continue;
					case IPropertySymbol property when property.Type is INamedTypeSymbol propertyType && propertyType.IsPXAction():
						yield return propertyType;
						continue;
				}
			}
		}
		#endregion

		#region PXAction Symbol With Type
		/// <summary>
		/// Gets the PXAction symbols with types from graph and, if <paramref name="includeActionsFromInheritanceChain"/> is <c>true</c>, its base graphs.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromInheritanceChain">(Optional) True to include, false to exclude the actions from the inheritance chain.</param>
		/// <returns/>
		public static ActionSymbolWithTypeCollection GetPXActionSymbolsWithTypesFromGraph(this ITypeSymbol graph, PXContext pxContext,
																						  bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph?.InheritsFrom(pxContext.PXGraphType) != true)
				return Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

			if (includeActionsFromInheritanceChain)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => baseGraph.GetPXActionSymbolsWithTypesFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
				return graph.GetPXActionSymbolsWithTypesFromGraphOrGraphExtensionImpl(pxContext);
		}

		/// <summary>
		/// Gets the PXAction symbols with types from graph extension and its base graph extensions if there is a class hierarchy and
		/// <paramref name="includeActionsFromInheritanceChain"/> parameter is <c>true</c>.
		/// Does not include actions from extension's graph.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromInheritanceChain">(Optional) True to include, false to exclude the actions from inheritance chain.</param>
		/// <returns/>
		public static ActionSymbolWithTypeCollection GetPXActionSymbolsWithTypesFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext,
																								   bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
				return Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

			if (includeActionsFromInheritanceChain)
			{
				return graphExtension.GetBaseTypesAndThis()
									 .TakeWhile(baseGraphExt => !baseGraphExt.IsGraphExtensionBaseType())
									 .Reverse()
									 .SelectMany(baseGraphExt => baseGraphExt.GetPXActionSymbolsWithTypesFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
				return graphExtension.GetPXActionSymbolsWithTypesFromGraphOrGraphExtensionImpl(pxContext);
		}

		/// <summary>
		/// Gets the PXAction symbols with types declared on the graph extension and its base graph.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromExtensionInheritanceChain">(Optional) True to include, false to exclude the actions from extension inheritance chain.</param>
		/// <param name="includeActionsFromGraphInheritanceChain">(Optional) True to include, false to exclude the actions from graph inheritance chain.</param>
		/// <returns/>
		public static ActionSymbolWithTypeCollection GetPXActionSymbolsWithTypesFromGraphExtensionAndItsBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext,
																												  bool includeActionsFromExtensionInheritanceChain = true,
																												  bool includeActionsFromGraphInheritanceChain = true)
		{
			var extensionActionsWithTypes = graphExtension.GetPXActionSymbolsWithTypesFromGraphExtension(pxContext,
																										 includeActionsFromExtensionInheritanceChain);
			ITypeSymbol graph = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graph == null)
				return extensionActionsWithTypes;

			var graphActionsWithTypes = graph.GetPXActionSymbolsWithTypesFromGraph(pxContext,
																				   includeActionsFromGraphInheritanceChain);
			return graphActionsWithTypes.Concat(extensionActionsWithTypes);
		}

		private static ActionSymbolWithTypeCollection GetPXActionSymbolsWithTypesFromGraphOrGraphExtensionImpl(this ITypeSymbol graphOrExtension,
																											   PXContext pxContext)
		{
			foreach (ISymbol member in graphOrExtension.GetMembers())
			{
				switch (member)
				{
					case IFieldSymbol field when field.Type is INamedTypeSymbol fieldType && fieldType.IsPXAction():
						yield return (field, fieldType);
						continue;
					case IPropertySymbol property when property.Type is INamedTypeSymbol propertyType && propertyType.IsPXAction():
						yield return (property, propertyType);
						continue;
				}
			}
		}
		#endregion

		public static bool IsDelegateForViewInPXGraph(this IMethodSymbol method, PXContext pxContext)
		{
			if (method == null || method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable)
				return false;

			INamedTypeSymbol containingType = method.ContainingType;

			if (containingType == null ||
			   (!containingType.InheritsFrom(pxContext.PXGraphType) && !containingType.InheritsFrom(pxContext.PXGraphExtensionType)))
				return false;

			return containingType.GetMembers()
								 .OfType<IFieldSymbol>()
								 .Where(field => field.Type.InheritsFrom(pxContext.PXSelectBaseType))
								 .Any(field => String.Equals(field.Name, method.Name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Get view's DAC for which the view was declared.
		/// </summary>
		/// <param name="pxView">The view to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The DAC from view.
		/// </returns>
		public static ITypeSymbol GetDacFromView(this ITypeSymbol pxView, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (pxView?.InheritsFrom(pxContext.PXSelectBaseType) != true)
				return null;

			INamedTypeSymbol baseViewType = pxView.GetBaseTypesAndThis()
												  .OfType<INamedTypeSymbol>()
												  .FirstOrDefault(type => !type.IsCustomBqlCommand(pxContext));

			if (baseViewType?.IsBqlCommand() != true || baseViewType.TypeArguments.Length == 0)
				return null;

			return baseViewType.TypeArguments[0];
		}

		/// <summary>
		/// Get action's DAC for which the action was declared.
		/// </summary>
		/// <param name="pxAction">The action to act on.</param>
		/// <returns>
		/// The DAC from action.
		/// </returns>
		public static ITypeSymbol GetDacFromAction(this INamedTypeSymbol pxAction)
		{
			if (pxAction?.IsPXAction() != true)
				return null;

			ImmutableArray<ITypeSymbol> actionTypeArgs = pxAction.TypeArguments;

			if (actionTypeArgs.Length == 0)  
				return null;

			ITypeSymbol pxActionDacType = actionTypeArgs[0];
			return pxActionDacType.IsDAC()
				? pxActionDacType
				: null;
		}

		/// <summary>
		/// Get declared primary DAC from graph or graph extension.
		/// </summary>
		/// <param name="graphOrExtension">The graph or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The declared primary DAC from graph or graph extension.
		/// </returns>
		public static ITypeSymbol GetDeclaredPrimaryDacFromGraphOrGraphExtension(this INamedTypeSymbol graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			bool isGraph = graphOrExtension?.InheritsFrom(pxContext.PXGraphType) ?? false;

			if (!isGraph && !graphOrExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
				return null;

			ITypeSymbol graph = isGraph
				? graphOrExtension
				: graphOrExtension.GetGraphFromGraphExtension(pxContext);

			var baseGraphType = graph.GetBaseTypesAndThis()
									 .OfType<INamedTypeSymbol>()
									 .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type)) as INamedTypeSymbol;

			if (baseGraphType == null || baseGraphType.TypeArguments.Length < 2)
				return null;

			ITypeSymbol primaryDacType = baseGraphType.TypeArguments[1];
			return primaryDacType.IsDAC() ? primaryDacType : null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsGraphOrGraphExtensionBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraph ||
				   typeNameWithoutGenericArgsCount == TypeNames.PXGraphExtension;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsGraphBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraph;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGraphExtensionBaseType(this ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount == TypeNames.PXGraphExtension;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsGraphWithPrimaryDacBaseGenericType(INamedTypeSymbol type) =>
			type.TypeArguments.Length >= 2 && type.Name == TypeNames.PXGraph;

        public static (MethodDeclarationSyntax Node, IMethodSymbol Symbol) GetGraphExtensionInitialization
            (this INamedTypeSymbol typeSymbol, PXContext pxContext, CancellationToken cancellation = default)
        {
            typeSymbol.ThrowOnNull(nameof(typeSymbol));

            IMethodSymbol initialize = typeSymbol.GetMembers()
                                       .OfType<IMethodSymbol>()
                                       .Where(m => pxContext.PXGraphExtensionInitializeMethod.Equals(m.OverriddenMethod))
                                       .FirstOrDefault();
            if (initialize == null)
                return (null, null);

            SyntaxReference reference = initialize.DeclaringSyntaxReferences.FirstOrDefault();
            if (reference == null)
                return (null, null);

            if (!(reference.GetSyntax(cancellation) is MethodDeclarationSyntax node))
                return (null, null);

            return (node, initialize);
        }
	}
}
