using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers;



namespace Acuminator.Utilities
{
	public static class GraphSymbolUtils
	{
		public static ITypeSymbol GetGraphFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension?.BaseType == null || !graphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return null;

			var graphExtTypeArgs = graphExtension.BaseType.TypeArguments;

			if (graphExtTypeArgs.Length == 0)
				return null;

			ITypeSymbol firstTypeArg = graphExtTypeArgs[0];

			if (!(firstTypeArg is ITypeSymbol pxGraph) || !pxGraph.IsPXGraph())
				return null;

			return pxGraph;
		}

		public static IEnumerable<INamedTypeSymbol> GetAllViewTypesFromPXGraphOrPXGraphExtension(this ITypeSymbol graphOrExtension, 
																								 PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphOrExtension == null ||
			   (!graphOrExtension.InheritsFrom(pxContext.PXGraphType) && !graphOrExtension.InheritsFrom(pxContext.PXGraphExtensionType)))
			{
				return Enumerable.Empty<INamedTypeSymbol>();
			}

			return GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(graphOrExtension, pxContext);			
		}

		public static IEnumerable<INamedTypeSymbol> GetAllViewsFromThisAndBaseGraphs(this ITypeSymbol graph, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graph == null || !graph.InheritsFrom(pxContext.PXGraphType))
				return Enumerable.Empty<INamedTypeSymbol>();

			return graph.GetBaseTypesAndThis()
						.TakeWhile(baseGraph => !IsGraphOrGraphExtensionBaseType(baseGraph))
						.Reverse()
						.SelectMany(baseGraph => GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(baseGraph, pxContext));

			//****************************Local Function************************************************************
			bool IsGraphOrGraphExtensionBaseType(ITypeSymbol type)
			{
				string typeNameWithoutGenericArgsCount = type.Name.Split('`')[0];
				return typeNameWithoutGenericArgsCount.Equals(TypeNames.PXGraph, StringComparison.Ordinal) ||
					   typeNameWithoutGenericArgsCount.Equals(TypeNames.PXGraphExtension, StringComparison.Ordinal);
			}
		}

		private static IEnumerable<INamedTypeSymbol> GetAllViewTypesFromPXGraphOrPXGraphExtensionImpl(ITypeSymbol graphOrExtension,
																									  PXContext pxContext)
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

		public static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraphOrGraphExtension(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphOrExtension == null ||
			   (!graphOrExtension.InheritsFrom(pxContext.PXGraphType) && !graphOrExtension.InheritsFrom(pxContext.PXGraphExtensionType)))
			{
				return Enumerable.Empty<INamedTypeSymbol>();
			}

			return graphOrExtension.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext);
		}

		public static IEnumerable<INamedTypeSymbol> GetPXActionsFromGraphExtensionAndItsBaseGraph(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
				return Enumerable.Empty<INamedTypeSymbol>();

			var graphExtensionActions = graphExtension.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext);
			var graph = graphExtension.GetGraphFromGraphExtension(pxContext);

			if (graph == null)
				return graphExtensionActions;

			var graphActions = graph.GetPXActionsFromGraphOrGraphExtensionImpl(pxContext);
			return graphActions.Concat(graphExtensionActions);
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
	}
}
