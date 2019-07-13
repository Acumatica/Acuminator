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

			return GetPropertiesOrFieldsInfoFromDacExtension<(PropertyDeclarationSyntax, IPropertySymbol)>(dacExtension, pxContext,
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
				return dac.GetDacWithBaseTypes()
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

		#region DAC fields
		/// <summary>
		/// Get the DAC fields symbols and syntax nodes from the DAC.
		/// </summary>
		/// <param name="dac">The DAC to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeFromInheritanceChain">(Optional) True to include, false to exclude the DAC fields from the inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// The DAC fields from DAC.
		/// </returns>
		public static DacFieldOverridableCollection GetDacFieldsFromDac(this ITypeSymbol dac, PXContext pxContext,
																		bool includeFromInheritanceChain = true,
																		CancellationToken cancellation = default)
		{
			dac.ThrowOnNull(nameof(dac));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!dac.IsDAC(pxContext))
				return Enumerable.Empty<OverridableItem<(ClassDeclarationSyntax, INamedTypeSymbol)>>();

			var dacFieldsByName = new OverridableItemsCollection<(ClassDeclarationSyntax Node, INamedTypeSymbol DacField)>();
			var dacFields = GetDacFieldsFromDacImpl(dac, pxContext, includeFromInheritanceChain, cancellation);

			dacFieldsByName.AddRangeWithDeclarationOrder(dacFields, startingOrder: 0, keySelector: p => p.DacField.Name);
			return dacFieldsByName.Items;
		}

		/// <summary>
		/// Get the DAC field symbols and syntax nodes from the DAC extension.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// The DAC fields from DAC extension and base DAC.
		/// </returns>
		public static DacFieldOverridableCollection GetDacFieldsFromDacExtensionAndBaseDac(this ITypeSymbol dacExtension, PXContext pxContext,
																						   CancellationToken cancellation)
		{
			dacExtension.ThrowOnNull(nameof(dacExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetPropertiesOrFieldsInfoFromDacExtension<(ClassDeclarationSyntax, INamedTypeSymbol)>(
				dacExtension, pxContext, AddFieldsFromDac, AddFieldsFromDacExtension);


			int AddFieldsFromDac(OverridableItemsCollection<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> fieldsCollection,
									 ITypeSymbol dac, int startingOrder)
			{
				var dacFields = dac.GetDacFieldsFromDacImpl(pxContext, includeFromInheritanceChain: true, cancellation);
				return fieldsCollection.AddRangeWithDeclarationOrder(dacFields, startingOrder, keySelector: h => h.Symbol.Name);
			}

			int AddFieldsFromDacExtension(OverridableItemsCollection<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> fieldsCollection,
											   ITypeSymbol dacExt, int startingOrder)
			{
				var dacExtensionFields = dacExt.GetDacFieldsFromDacOrDacExtension(pxContext, cancellation);
				return fieldsCollection.AddRangeWithDeclarationOrder(dacExtensionFields, startingOrder, keySelector: h => h.Symbol.Name);
			}
		}

		private static IEnumerable<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> GetDacFieldsFromDacImpl(this ITypeSymbol dac,
																								PXContext pxContext, bool includeFromInheritanceChain,
																								CancellationToken cancellation)
		{
			if (includeFromInheritanceChain)
			{
				return dac.GetDacWithBaseTypes()
						  .Reverse()
						  .SelectMany(baseGraph => GetDacFieldsFromDacOrDacExtension(baseGraph, pxContext, cancellation));
			}
			else
			{
				return GetDacFieldsFromDacOrDacExtension(dac, pxContext, cancellation);
			}
		}

		private static IEnumerable<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> GetDacFieldsFromDacOrDacExtension(
																								this ITypeSymbol dacOrDacExtension,
																								PXContext pxContext, CancellationToken cancellation)
		{
			IEnumerable<INamedTypeSymbol> dacFields = dacOrDacExtension.GetMembers()
																	   .OfType<INamedTypeSymbol>()
																	   .Where(type => type.IsDacField(pxContext));												 
			foreach (INamedTypeSymbol field in dacFields)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxReference reference = field.DeclaringSyntaxReferences.FirstOrDefault();

				if (reference?.GetSyntax(cancellation) is ClassDeclarationSyntax declaration)
				{
					yield return (declaration, field);
				}
			}
		}
		#endregion

		private static IEnumerable<OverridableItem<T>> GetPropertiesOrFieldsInfoFromDacExtension<T>(ITypeSymbol dacExtension, PXContext pxContext,
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
