using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DacPropertyOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax Node, Microsoft.CodeAnalysis.IPropertySymbol DacProperty)>>;

using DacFieldOverridableCollection = 
	System.Collections.Generic.IEnumerable<
		Acuminator.Utilities.Roslyn.Semantic.OverridableItem<(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax Node, Microsoft.CodeAnalysis.INamedTypeSymbol FieldType)>>;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public static class DacPropertySymbolUtils
    {
        /// <summary>
        /// A delegate type for a DAC property which extracts info DTOs about DAC properties/fields from <paramref name="dacOrDacExtension"/> 
        /// and adds them to the <paramref name="dacInfos"/> collection with a consideration for DAC properties/fields declaration order.
        /// Returns the number following the last assigned declaration order.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="dacPropertyInfos">The DAC property infos.</param>
        /// <param name="dacOrDacExtension">The DAC or DAC extension.</param>
        /// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
        /// <returns/>
        private delegate int AddDacPropertyInfoWithOrderDelegate<T>(OverridableItemsCollection<T> dacPropertyInfos,
																    ITypeSymbol dacOrDacExtension, int startingOrder);


		#region Dac Properties
        /// <summary>
        /// Gets the DAC property symbols and syntax nodes from DAC and, if <paramref name="includeFromInheritanceChain"/> is <c>true</c>, its base DACs.
        /// </summary>
        /// <param name="dac">The DAC to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="includeFromInheritanceChain">(Optional) True to include, false to exclude the properties from the inheritance chain.</param>
        /// <param name="cancellation">(Optional) Cancellation token.</param>
        /// <returns>The DAC property symbols with nodes from DAC.</returns>
		public static DacPropertyOverridableCollection GetDacPropertySymbolsWithNodesFromDac(this ITypeSymbol dac, PXContext pxContext,
																					         bool includeFromInheritanceChain = true,
                                                                                             CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!dac.IsDAC(pxContext))
				return Enumerable.Empty<OverridableItem<(PropertyDeclarationSyntax, IPropertySymbol)>>();

			var propertiesByName = new OverridableItemsCollection<(PropertyDeclarationSyntax Node, IPropertySymbol DacProperty)>();
			var dacProperties = GetPropertiesFromDacImpl(dac, pxContext, includeFromInheritanceChain, cancellation);

            propertiesByName.AddRangeWithDeclarationOrder(dacProperties, startingOrder: 0, keySelector: p => p.DacProperty.Name);
			return propertiesByName.Items;
		}

        /// <summary>
        /// Get all properties from DAC or DAC extension and its base DACs and base DAC extensions (extended via Acumatica Customization).
        /// </summary>
        /// <param name="dacOrExtension">The DAC or DAC extension to act on.</param>
        /// <param name="pxContext">Context.</param>
        /// <param name="cancellation">(Optional) Cancellation token.</param>
        /// <returns>The properties from DAC or DAC extension and base DAC.</returns>
		public static DacPropertyOverridableCollection GetPropertiesFromDacOrDacExtensionAndBaseDac(this ITypeSymbol dacOrExtension,
																								    PXContext pxContext, 
                                                                                                    CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			bool isDac = dacOrExtension.IsDAC(pxContext);

			if (!isDac && !dacOrExtension.IsDacExtension(pxContext))
				return Enumerable.Empty<OverridableItem<(PropertyDeclarationSyntax, IPropertySymbol)>>();

			return isDac
				? dacOrExtension.GetDacPropertySymbolsWithNodesFromDac(pxContext)
				: dacOrExtension.GetPropertiesFromDacExtensionAndBaseDac(pxContext);
		}

		/// <summary>
		/// Get DAC properties from the DAC extension and its base DAC.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on</param>
		/// <param name="pxContext">Context</param>
		/// <returns/>
		public static DacPropertyOverridableCollection GetPropertiesFromDacExtensionAndBaseDac(this ITypeSymbol dacExtension, PXContext pxContext,
                                                                                               CancellationToken cancellation = default)
		{
			dacExtension.ThrowOnNull(nameof(dacExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetPropertiesInfoFromDacExtension<(PropertyDeclarationSyntax, IPropertySymbol)>(dacExtension, pxContext,
																								   AddPropertiesFromDac, AddPropertiesFromDacExtension);

			//-----------------------Local function----------------------------------------
			int AddPropertiesFromDac(OverridableItemsCollection<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> propertiesCollection,
									 ITypeSymbol dac, int startingOrder)
			{
				var dacProperties = dac.GetPropertiesFromDacImpl(pxContext, includeFromInheritanceChain: true, cancellation);
				return propertiesCollection.AddRangeWithDeclarationOrder(dacProperties, startingOrder,
																		 keySelector: dacProperty => dacProperty.Symbol.Name);
			}

			int AddPropertiesFromDacExtension(OverridableItemsCollection<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> propertiesCollection,
											  ITypeSymbol dacExt, int startingOrder)
			{
				var dacExtensionProperties = GetPropertiesFromDacOrDacExtensionImpl(dacExt, pxContext, cancellation);
				return propertiesCollection.AddRangeWithDeclarationOrder(dacExtensionProperties, startingOrder,
																		 keySelector: dacProperty => dacProperty.Symbol.Name);
			}
		}

		private static IEnumerable<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> GetPropertiesFromDacImpl(this ITypeSymbol dac, 
                                                                                                                      PXContext pxContext,
																			                                          bool includeFromInheritanceChain,
                                                                                                                      CancellationToken cancellation)
		{
			if (includeFromInheritanceChain)
			{
				return dac.GetBaseTypesAndThis()
						  .TakeWhile(type => !type.IsDacBaseType())
						  .Reverse()
						  .SelectMany(baseDac => baseDac.GetPropertiesFromDacOrDacExtensionImpl(pxContext, cancellation));
			}
			else
			{
				return dac.GetPropertiesFromDacOrDacExtensionImpl(pxContext, cancellation);
			}
		}

		private static IEnumerable<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> GetPropertiesFromDacOrDacExtensionImpl(this ITypeSymbol dacOrExtension, 
                                                                                                                                    PXContext pxContext,
                                                                                                                                    CancellationToken cancellation)
		{
            var dacProperties = dacOrExtension.GetMembers().OfType<IPropertySymbol>()
                                                           .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

            foreach (IPropertySymbol property in dacProperties)
			{
                cancellation.ThrowIfCancellationRequested();

                if (property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellation) is PropertyDeclarationSyntax node)
                {
                    yield return (node, property);
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

			return GetPropertiesInfoFromDacExtension<(MethodDeclarationSyntax, IMethodSymbol)>(
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

		private static IEnumerable<OverridableItem<T>> GetPropertiesInfoFromDacExtension<T>(ITypeSymbol dacExtension, PXContext pxContext,
															AddDacPropertyInfoWithOrderDelegate<T> addDacPropertyInfoWithOrder,
															AddDacPropertyInfoWithOrderDelegate<T> addDacExtensionPropertyInfoWithOrder)
		{
			var empty = Enumerable.Empty<OverridableItem<T>>();

			if (!dacExtension.IsDacExtension(pxContext))
				return empty;

			var dacType = dacExtension.GetDacFromDacExtension(pxContext);

			if (dacType == null)
				return empty;

			var allExtensionsFromBaseToDerived = dacExtension.GetDacExtensionWithBaseExtensions(pxContext, SortDirection.Ascending,
																								includeDac: false);
			if (allExtensionsFromBaseToDerived.IsNullOrEmpty())
				return empty;

			var propertiesByName = new OverridableItemsCollection<T>();
			int declarationOrder = addDacPropertyInfoWithOrder(propertiesByName, dacType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addDacExtensionPropertyInfoWithOrder(propertiesByName, extension, declarationOrder);
			}

			return propertiesByName.Items;
		}
	}
}
