using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ActionHandlersOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax Node, Microsoft.CodeAnalysis.IMethodSymbol Symbol)>>;

using ActionsOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>>;

using ActionSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphActionSymbolUtils
	{
		/// <summary>
		/// A delegate type for an action which extracts info DTOs about graph actions/action handlers from <paramref name="graphOrgraphExtension"/> 
		/// and adds them to the <paramref name="actionInfos"/> collection with account for actions/action handlers declaration order.
		/// Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="actionInfos">The action infos.</param>
		/// <param name="graphOrgraphExtension">The graph orgraph extension.</param>
		/// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
		/// <returns/>
		private delegate int AddActionInfoWithOrderDelegate<T>(OverridableItemsCollection<T> actionInfos,
																ITypeSymbol graphOrgraphExtension, int startingOrder);


		#region ActionsGet
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
				return Enumerable.Empty<OverridableItem<(ISymbol, INamedTypeSymbol)>>();

			var actionsByName = new OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)>();
			var graphActions = GetActionsFromGraphImpl(graph, pxContext, includeActionsFromInheritanceChain);

			actionsByName.AddRangeWithDeclarationOrder(graphActions, startingOrder: 0, keySelector: action => action.Symbol.Name);
			return actionsByName.Items;
		}

		/// <summary>
		/// Get all actions from graph or graph extension and its base graphs and base graph extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="graphOrExtension">The graph Or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns/>
		public static ActionsOverridableCollection GetActionsFromGraphOrGraphExtensionAndBaseGraph(this ITypeSymbol graphOrExtension,
																								   PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			graphOrExtension.ThrowOnNull(nameof(graphOrExtension));

			bool isGraph = graphOrExtension.IsPXGraph(pxContext);

			if (!isGraph && !graphOrExtension.IsPXGraphExtension(pxContext))
				return Enumerable.Empty<OverridableItem<(ISymbol, INamedTypeSymbol)>>();

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


			int AddActionsFromGraph(OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)> actionsCollection,
									ITypeSymbol graph, int startingOrder)
			{
				var graphActions = graph.GetActionsFromGraphImpl(pxContext, includeActionsFromInheritanceChain: true);
				return actionsCollection.AddRangeWithDeclarationOrder(graphActions, startingOrder,
																	  keySelector: action => action.Symbol.Name);
			}

			int AddActionsFromGraphExtension(OverridableItemsCollection<(ISymbol Symbol, INamedTypeSymbol Type)> actionsCollection,
											 ITypeSymbol graphExt, int startingOrder)
			{
				var extensionActions = GetActionsFromGraphOrGraphExtensionImpl(graphExt, pxContext);
				return actionsCollection.AddRangeWithDeclarationOrder(extensionActions, startingOrder,
																	  keySelector: action => action.Symbol.Name);
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
				return Enumerable.Empty<OverridableItem<(MethodDeclarationSyntax, IMethodSymbol)>>();
			}

			var actionHandlersByName = new OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();
			var graphHandlers = GetActionHandlersFromGraphImpl(graph, actionsByName, pxContext, cancellation, inheritance);

			actionHandlersByName.AddRangeWithDeclarationOrder(graphHandlers, startingOrder: 0, keySelector: h => h.Symbol.Name);
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


			int AddHandlersFromGraph(OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> handlersCollection,
									  ITypeSymbol graph, int startingOrder)
			{
				var graphHandlers = graph.GetActionHandlersFromGraphImpl(actionsByName, pxContext,
																		 cancellation, inheritance: true);
				return handlersCollection.AddRangeWithDeclarationOrder(graphHandlers, startingOrder, keySelector: h => h.Symbol.Name);
			}

			int AddHandlersFromGraphExtension(OverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> handlersCollection,
											   ITypeSymbol graphExt, int startingOrder)
			{
				var extensionHandlers = graphExt.GetActionHandlersFromGraphOrGraphExtension(actionsByName, pxContext, cancellation);
				return handlersCollection.AddRangeWithDeclarationOrder(extensionHandlers, startingOrder, keySelector: h => h.Symbol.Name);
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

		private static IEnumerable<OverridableItem<T>> GetActionInfoFromGraphExtension<T>(ITypeSymbol graphExtension, PXContext pxContext,
															AddActionInfoWithOrderDelegate<T> addGraphActionInfoWithOrder,
															AddActionInfoWithOrderDelegate<T> addGraphExtensionActionInfoWithOrder)
		{
			var empty = Enumerable.Empty<OverridableItem<T>>();

			if (!graphExtension.InheritsFrom(pxContext.PXGraphExtensionType) || !graphExtension.BaseType.IsGenericType)
				return Enumerable.Empty<OverridableItem<T>>();

			var graphType = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graphType == null)
				return empty;

			var allExtensionsFromBaseToDerived = graphExtension.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending,
																									includeGraph: false);
			if (allExtensionsFromBaseToDerived.IsNullOrEmpty())
				return empty;

			var actionsByName = new OverridableItemsCollection<T>();
			int declarationOrder = addGraphActionInfoWithOrder(actionsByName, graphType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addGraphExtensionActionInfoWithOrder(actionsByName, extension, declarationOrder);
			}

			return actionsByName.Items;
		}
	}
}
