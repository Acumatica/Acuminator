using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ActionSymbolWithTypeCollection = System.Collections.Generic.IEnumerable<(Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ActionType)>;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphActionSymbolUtils
	{
		private const int EstimatedNumberOfActionsInGraph = 8;

		/// <summary>
		/// A delegate type for an action which extracts info DTOs about graph actions/action handlers from <paramref name="graphOrGraphExtension"/> 
		/// and adds them to the <paramref name="actionInfos"/> collection with a consideration for actions/action handlers declaration order.
		/// Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="TInfo">Generic type parameter representing overridable info type.</typeparam>
		/// <param name="actionInfos">The action infos.</param>
		/// <param name="graphOrGraphExtension">The graph or graph extension.</param>
		/// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
		/// <returns/>
		private delegate int AddActionInfoWithOrderDelegate<TInfo>(OverridableItemsCollection<TInfo> actionInfos,
																   ITypeSymbol graphOrGraphExtension, int startingOrder)
		where TInfo : IOverridableItem<TInfo>;


		#region ActionsGet
		/// <summary>
		/// Gets the PXAction symbols with types from graph and, if <paramref name="includeActionsFromInheritanceChain"/> is <c>true</c>, its base graphs.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeActionsFromInheritanceChain">(Optional) True to include, false to exclude the actions from the inheritance chain.</param>
		/// <returns/>
		public static OverridableItemsCollection<ActionInfo> GetActionSymbolsWithTypesFromGraph(this ITypeSymbol graph, PXContext pxContext,
																								bool includeActionsFromInheritanceChain = true)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!graph.IsPXGraph(pxContext))
				return new OverridableItemsCollection<ActionInfo>();

			var actionsByName = new OverridableItemsCollection<ActionInfo>(capacity: EstimatedNumberOfActionsInGraph);
			var graphActions = GetRawActionsFromGraphImpl(graph, pxContext, includeActionsFromInheritanceChain);
			var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(pxContext);

			actionsByName.AddRangeWithDeclarationOrder(graphActions, startingOrder: 0,
													   (action, order) => new ActionInfo(action.ActionSymbol, action.ActionType, order, 
																						 systemActionsRegister.IsSystemAction(action.ActionType)));
			return actionsByName;
		}

		/// <summary>
		/// Get all actions from graph or graph extension and its base graphs and base graph extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="graphOrExtension">The graph Or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns/>
		public static OverridableItemsCollection<ActionInfo> GetActionsFromGraphOrGraphExtensionAndBaseGraph(this ITypeSymbol graphOrExtension,
																											 PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			graphOrExtension.ThrowOnNull(nameof(graphOrExtension));

			bool isGraph = graphOrExtension.IsPXGraph(pxContext);

			if (!isGraph && !graphOrExtension.IsPXGraphExtension(pxContext))
				return new OverridableItemsCollection<ActionInfo>();

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
		public static OverridableItemsCollection<ActionInfo> GetActionsFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(pxContext);
			return GetActionInfoFromGraphExtension<ActionInfo>(graphExtension, pxContext, AddActionsFromGraph, AddActionsFromGraphExtension);


			int AddActionsFromGraph(OverridableItemsCollection<ActionInfo> actionsCollection, ITypeSymbol graph, int startingOrder)
			{
				var graphActions = graph.GetRawActionsFromGraphImpl(pxContext, includeActionsFromInheritanceChain: true);
				return actionsCollection.AddRangeWithDeclarationOrder(graphActions, startingOrder,
																	  (action, order) => new ActionInfo(action.ActionSymbol, action.ActionType, order,
																										systemActionsRegister.IsSystemAction(action.ActionType)));
			}

			int AddActionsFromGraphExtension(OverridableItemsCollection<ActionInfo> actionsCollection, ITypeSymbol graphExt, int startingOrder)
			{
				var extensionActions = GetRawActionsFromGraphOrGraphExtensionImpl(graphExt, pxContext);
				return actionsCollection.AddRangeWithDeclarationOrder(extensionActions, startingOrder,
																	 (action, order) => new ActionInfo(action.ActionSymbol, action.ActionType, order,
																										systemActionsRegister.IsSystemAction(action.ActionType)));
			}
		}

		private static ActionSymbolWithTypeCollection GetRawActionsFromGraphImpl(this ITypeSymbol graph, PXContext pxContext,
																			  bool includeActionsFromInheritanceChain)
		{
			if (includeActionsFromInheritanceChain)
			{
				return graph.GetGraphWithBaseTypes()
							.Reverse()
							.SelectMany(baseGraph => baseGraph.GetRawActionsFromGraphOrGraphExtensionImpl(pxContext));
			}
			else
			{
				return graph.GetRawActionsFromGraphOrGraphExtensionImpl(pxContext);
			}
		}

		private static ActionSymbolWithTypeCollection GetRawActionsFromGraphOrGraphExtensionImpl(this ITypeSymbol graphOrExtension, PXContext pxContext)
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
		/// Get the action handlers method symbols and syntax nodes from the graph. The <paramref name="actionsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="actionsByName">The actions of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="inheritance">(Optional) If true includes action handlers from the graph inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// The action handlers from graph.
		/// </returns>
		public static OverridableItemsCollection<ActionHandlerInfo> GetActionHandlersFromGraph(this ITypeSymbol graph, IDictionary<string, ActionInfo> actionsByName,
																							   PXContext pxContext, bool inheritance = true, 
																							   CancellationToken cancellation = default)
		{
			graph.ThrowOnNull(nameof(graph));
			actionsByName.ThrowOnNull(nameof(actionsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!graph.IsPXGraph(pxContext))
			{
				return new OverridableItemsCollection<ActionHandlerInfo>();
			}

			var actionHandlersByName = new OverridableItemsCollection<ActionHandlerInfo>(capacity: EstimatedNumberOfActionsInGraph);
			var graphHandlers = GetRawActionHandlersFromGraphImpl(graph, actionsByName, pxContext, cancellation, inheritance);

			actionHandlersByName.AddRangeWithDeclarationOrder(graphHandlers, startingOrder: 0, 
															  (hander, order) => new ActionHandlerInfo(hander.Node, hander.Symbol, order));
			return actionHandlersByName;
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
		public static OverridableItemsCollection<ActionHandlerInfo> GetActionHandlersFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension,
																							IDictionary<string, ActionInfo> actionsByName, PXContext pxContext,
																							CancellationToken cancellation)
		{
			graphExtension.ThrowOnNull(nameof(graphExtension));
			actionsByName.ThrowOnNull(nameof(actionsByName));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetActionInfoFromGraphExtension<ActionHandlerInfo>(graphExtension, pxContext, AddHandlersFromGraph, AddHandlersFromGraphExtension);


			int AddHandlersFromGraph(OverridableItemsCollection<ActionHandlerInfo> handlersCollection, ITypeSymbol graph, int startingOrder)
			{
				var graphHandlers = graph.GetRawActionHandlersFromGraphImpl(actionsByName, pxContext,
																		 cancellation, inheritance: true);
				return handlersCollection.AddRangeWithDeclarationOrder(graphHandlers, startingOrder,
																	   (hander, order) => new ActionHandlerInfo(hander.Node, hander.Symbol, order));
			}

			int AddHandlersFromGraphExtension(OverridableItemsCollection<ActionHandlerInfo> handlersCollection, ITypeSymbol graphExt, int startingOrder)
			{
				var extensionHandlers = graphExt.GetRawActionHandlersFromGraphOrGraphExtension(actionsByName, pxContext, cancellation);
				return handlersCollection.AddRangeWithDeclarationOrder(extensionHandlers, startingOrder,
																	   (hander, order) => new ActionHandlerInfo(hander.Node, hander.Symbol, order));
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetRawActionHandlersFromGraphImpl(
																this ITypeSymbol graph, IDictionary<string, ActionInfo> actionsByName,
																PXContext pxContext, CancellationToken cancellation, bool inheritance)
		{
			if (inheritance)
			{
				return graph.GetGraphWithBaseTypes()
							.Reverse()
							.SelectMany(baseGraph => GetRawActionHandlersFromGraphOrGraphExtension(baseGraph, actionsByName, pxContext, cancellation));
			}
			else
			{
				return GetRawActionHandlersFromGraphOrGraphExtension(graph, actionsByName, pxContext, cancellation);
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> GetRawActionHandlersFromGraphOrGraphExtension(
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

		private static OverridableItemsCollection<TInfo> GetActionInfoFromGraphExtension<TInfo>(ITypeSymbol graphExtension, PXContext pxContext,
																		AddActionInfoWithOrderDelegate<TInfo> addGraphActionInfoWithOrder,
																		AddActionInfoWithOrderDelegate<TInfo> addGraphExtensionActionInfoWithOrder)
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

			var actionsByName = new OverridableItemsCollection<TInfo>(capacity: EstimatedNumberOfActionsInGraph);
			int declarationOrder = addGraphActionInfoWithOrder(actionsByName, graphType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addGraphExtensionActionInfoWithOrder(actionsByName, extension, declarationOrder);
			}

			return actionsByName;
		}
	}
}
