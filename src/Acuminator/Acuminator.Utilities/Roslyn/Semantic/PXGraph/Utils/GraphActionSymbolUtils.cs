using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ActionHandlersOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax Node, Microsoft.CodeAnalysis.IMethodSymbol Symbol)>>;
using ActionsOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType)>>;
using ActionSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>;


namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphActionSymbolUtils
	{
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

			if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
				return Enumerable.Empty<(ISymbol, INamedTypeSymbol)>();

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

		private static ActionSymbolWithTypeCollection GetPXActionsFromGraphOrGraphExtensionImpl(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			foreach (IFieldSymbol field in graphOrExtension.GetMembers().OfType<IFieldSymbol>())
			{
				if (field.DeclaredAccessibility == Accessibility.Public && 
					field.Type is INamedTypeSymbol fieldType && fieldType.IsPXAction())
				{
					yield return (field, fieldType);
				}							
			}
		}

		private static IEnumerable<GraphOverridableItem<T>> GetActionInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
															Action<GraphOverridableItemsCollection<T>, ITypeSymbol> addGraphActionInfo,
															Action<GraphOverridableItemsCollection<T>, ITypeSymbol> addGraphExtensionActionInfo)
		{
			var empty = Enumerable.Empty<GraphOverridableItem<T>>();

			if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
				return Enumerable.Empty<GraphOverridableItem<T>>();
			
			var infoByAction = new GraphOverridableItemsCollection<T>();
			INamedTypeSymbol baseType = graphExtension.BaseType;
			ITypeSymbol graphType = baseType.TypeArguments[baseType.TypeArguments.Length - 1];

			if (!graphType.IsPXGraph(pxContext))
				return empty;

			addGraphActionInfo(infoByAction, graphType);

			if (baseType.TypeArguments.Length >= 2 && !AddInfoFromBaseExtensions())
				return empty;
			
			addGraphExtensionActionInfo(infoByAction, graphExtension);
			return infoByAction.Items;


			//-----------------------------------------------------------------Local Function----------------------------------------------
			bool AddInfoFromBaseExtensions()
			{
				for (int i = baseType.TypeArguments.Length - 2; i >= 0; i--)
				{
					var argType = baseType.TypeArguments[i];

					if (!argType.IsPXGraphExtension(pxContext))
					{
						return false;
					}

					addGraphExtensionActionInfo(infoByAction, argType);
				}

				return true;
			}
		}		
	}
}
