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
		public static IEnumerable<INamedTypeSymbol> GetAllViewTypesFromPXGraphOrPXGraphExtension(this ITypeSymbol graphOrExtension, 
																								 PXContext pxContext)
		{
			if (graphOrExtension == null || 
			   (!graphOrExtension.InheritsFrom(pxContext.PXGraphType) && !graphOrExtension.InheritsFrom(pxContext.PXGraphExtensionType)))
				yield break;

			foreach (ISymbol member in graphOrExtension.GetMembers())
			{
				switch (member)
				{
					case IFieldSymbol field 
					when field.Type is INamedTypeSymbol fieldType && fieldType.InheritsFrom(pxContext.PXSelectBaseType):
						yield return fieldType;
						continue;
					case IPropertySymbol property 
					when property is INamedTypeSymbol propertyType && propertyType.InheritsFrom(pxContext.PXSelectBaseType):
						yield return propertyType;
						continue;
				}
			}								 
		}

		public static IEnumerable<ITypeSymbol> GetPXActionsFromGraph()
		{

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
	}
}
