using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ActionHandlersOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax Node, Microsoft.CodeAnalysis.IMethodSymbol Symbol)>>;
using ActionsOverridableCollection = System.Collections.Generic.IEnumerable<Acuminator.Utilities.Roslyn.Semantic.PXGraph.GraphOverridableItem<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>>;
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
		public static ActionsOverridableCollection GetActionSymbolsWithTypesFromGraph(this ITypeSymbol graph, PXContext pxContext,
																					  bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph?.InheritsFrom(pxContext.PXGraph.Type) != true)
				return Enumerable.Empty<GraphOverridableItem<(ISymbol, INamedTypeSymbol)>>();

			var actionsByName = new GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)>();
			GetActionsFromGraphImpl(graph, pxContext, includeActionsFromInheritanceChain)
				.ForEach(action => actionsByName.Add(action.ActionSymbol.Name, action));

			return actionsByName.Items;
		}

		/// <summary>
		/// Get all actions from graph or graph extension and its base graphs and base graph extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="graphOrExtension">The graph Or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns/>
		public static ActionsOverridableCollection GetActionsFromGraphOrGraphExtensionAndBaseGraph(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			graphOrExtension.ThrowOnNull(nameof(graphOrExtension));

			bool isGraph = graphOrExtension.IsPXGraph(pxContext);

			if (!isGraph && !graphOrExtension.IsPXGraphExtension(pxContext))
				return Enumerable.Empty<GraphOverridableItem<(ISymbol, INamedTypeSymbol)>>();

			return isGraph
				? graphOrExtension.GetActionSymbolsWithTypesFromGraph(pxContext)
				: graphOrExtension.GetActionsFromGraphExtensionAndBaseGraph(pxContext);
		}
		
		/// <summary>
		/// Get all action symbols and types from the graph extension and its base graph
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="pxContext">Context</param>
		/// <returns/>
		public static ActionsOverridableCollection GetActionsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetActionInfoFromGraphExtension<(ISymbol, INamedTypeSymbol)>(graphExtension, pxContext,
																				AddActionsFromGraph, AddActionsFromGraphExtension);

			//--------------------------------------------------------Local Functions----------------------------------------------------------
			void AddActionsFromGraph(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> actionsCollection, ITypeSymbol graph) =>
				graph.GetActionsFromGraphImpl(pxContext)
					 .ForEach(action => actionsCollection.Add(action.ActionSymbol.Name, action));

			void AddActionsFromGraphExtension(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> actionsCollection, ITypeSymbol graphExt)
			{
				var extActions = GetActionsFromGraphOrGraphExtensionImpl(graphExt, pxContext);

				extActions.ForEach(action => actionsCollection.Add(action.ActionSymbol.Name, action));
			}
		}

		private static ActionSymbolWithTypeCollection GetActionsFromGraphImpl(this ITypeSymbol graph, PXContext pxContext,
																			  bool includeActionsFromInheritanceChain = true)
		{
			if (includeActionsFromInheritanceChain)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => baseGraph.GetActionsFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
			{
				return graph.GetActionsFromGraphOrGraphExtensionImpl(pxContext);
			}
		}

		private static ActionSymbolWithTypeCollection GetActionsFromGraphOrGraphExtensionImpl(this ITypeSymbol graphOrExtension, PXContext pxContext)
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
			
			var actionsByName = new GraphOverridableItemsCollection<T>();
			INamedTypeSymbol baseType = graphExtension.BaseType;
			ITypeSymbol graphType = baseType.TypeArguments[baseType.TypeArguments.Length - 1];

			if (!graphType.IsPXGraph(pxContext))
				return empty;

			addGraphActionInfo(actionsByName, graphType);

			if (baseType.TypeArguments.Length >= 2 && !AddInfoFromBaseExtensions())
				return empty;
			
			addGraphExtensionActionInfo(actionsByName, graphExtension);
			return actionsByName.Items;


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

					addGraphExtensionActionInfo(actionsByName, argType);
				}

				return true;
			}
		}		
	}
}
