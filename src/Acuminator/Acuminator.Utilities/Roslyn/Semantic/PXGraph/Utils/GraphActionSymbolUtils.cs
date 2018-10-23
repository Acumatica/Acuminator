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
		#region Actions
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


			void AddActionsFromGraph(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> actionsCollection, ITypeSymbol graph) =>
				graph.GetActionsFromGraphImpl(pxContext, includeActionsFromInheritanceChain: true)
					 .ForEach(action => actionsCollection.Add(action.ActionSymbol.Name, action));

			void AddActionsFromGraphExtension(GraphOverridableItemsCollection<(ISymbol, INamedTypeSymbol)> actionsCollection, ITypeSymbol graphExt)
			{
				var extActions = GetActionsFromGraphOrGraphExtensionImpl(graphExt, pxContext);

				extActions.ForEach(action => actionsCollection.Add(action.ActionSymbol.Name, action));
			}
		}

		private static ActionSymbolWithTypeCollection GetActionsFromGraphImpl(this ITypeSymbol graph, PXContext pxContext,
																			  bool includeActionsFromInheritanceChain)
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
		#endregion

		#region Action Handlers
		/// <summary>
		/// Get the action handlers method symbols and syntax nodes from the graph. 
		/// The <paramref name="actionsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graph">The graph to act on</param>
		/// <param name="actionsByName">The actions of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <param name="inheritance">If true includes action handlers from the graph inheritance chain</param>
		/// <returns></returns>
		public static ActionHandlersOverridableCollection GetActionHandlersFromGraph(this ITypeSymbol graph, IDictionary<string, ActionInfo> actionsByName,
																					 PXContext pxContext, CancellationToken cancellation, 
																					 bool inheritance = true)
		{
			graph.ThrowOnNull(nameof(graph));
			actionsByName.ThrowOnNull(nameof(actionsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!graph.IsPXGraph(pxContext))
			{
				return Enumerable.Empty<GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)>>();
			}

			var actionHandlersByName = new GraphOverridableItemsCollection<(MethodDeclarationSyntax, IMethodSymbol)>();

			GetActionHandlersFromGraphImpl(graph, actionsByName, pxContext, cancellation, inheritance)
				.ForEach(handler => actionHandlersByName.Add(handler.Symbol.Name, handler));

			return actionHandlersByName.Items;
		}

		/// <summary>
		/// Get the action handlers symbols and syntax nodes from the graph extension.
		/// The <paramref name="actionsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="actionsByName">The actions of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns></returns>
		public static ActionHandlersOverridableCollection GetActionHandlersFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension,
																				IDictionary<string, ActionInfo> actionsByName, PXContext pxContext,
																				CancellationToken cancellation)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			actionsByName.ThrowOnNull(nameof(actionsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetActionInfoFromGraphExtension<(MethodDeclarationSyntax, IMethodSymbol)>(
				graphExtension, pxContext, AddHandlersFromGraph, AddHandlersFromGraphExtension);


			void AddHandlersFromGraph(GraphOverridableItemsCollection<(MethodDeclarationSyntax, IMethodSymbol)> handlersCollection, ITypeSymbol graph)
			{
				graph.GetActionHandlersFromGraphImpl(actionsByName, pxContext, cancellation, inheritance: true)
					 .ForEach(handler => handlersCollection.Add(handler.Symbol.Name, handler));
			}

			void AddHandlersFromGraphExtension(GraphOverridableItemsCollection<(MethodDeclarationSyntax, IMethodSymbol)> handlersCollection,
											   ITypeSymbol graphExt)
			{
				graphExt.GetActionHandlersFromGraphOrGraphExtension(actionsByName, pxContext, cancellation)
						.ForEach(handler => handlersCollection.Add(handler.Symbol.Name, handler));
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetActionHandlersFromGraphImpl(
																this ITypeSymbol graph, IDictionary<string, ActionInfo> actionsByName,
																PXContext pxContext, CancellationToken cancellation, bool inheritance)
		{
			if (inheritance)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetActionHandlersFromGraphOrGraphExtension(baseGraph, actionsByName, pxContext, cancellation));
			}
			else
			{
				return GetActionHandlersFromGraphOrGraphExtension(graph, actionsByName, pxContext, cancellation);
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetActionHandlersFromGraphOrGraphExtension(
															this ITypeSymbol graphOrExtension, IDictionary<string, ActionInfo> actionsByName, 
															PXContext pxContext, CancellationToken cancellation)
		{
			IEnumerable<IMethodSymbol> handlers = from method in graphOrExtension.GetMembers().OfType<IMethodSymbol>()
												  where method.IsValidActionHandler(pxContext) && actionsByName.ContainsKey(method.Name)
												  select method;
												  											 
			foreach (IMethodSymbol handler in handlers)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxReference reference = handler.DeclaringSyntaxReferences.FirstOrDefault();

				if (reference?.GetSyntax(cancellation) is MethodDeclarationSyntax declaration)
				{
					yield return (declaration, handler);
				}		
			}
		}
		#endregion

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
