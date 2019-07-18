﻿using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public static partial class DacPropertyAndFieldSymbolUtils
	{
		/// <summary>
		/// A delegate type for a DAC property which extracts info DTOs about DAC properties/fields from <paramref name="dacOrDacExtension"/> 
		/// and adds them to the <paramref name="dacInfos"/> collection with a consideration for DAC properties/fields declaration order.
		/// Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="TInfo">Generic type parameter representing overridable info type.</typeparam>
		/// <param name="dacPropertyInfos">The DAC property infos.</param>
		/// <param name="dacOrDacExtension">The DAC or DAC extension.</param>
		/// <param name="startingOrder">The declaration order which should be assigned to the first DTO.</param>
		/// <returns/>
		private delegate int AddDacPropertyInfoWithOrderDelegate<TInfo>(OverridableItemsCollection<TInfo> dacPropertyInfos,
																		ITypeSymbol dacOrDacExtension, int startingOrder)
		where TInfo : IOverridableItem<TInfo>;

		/// <summary>
		/// Gets the DAC property symbols and syntax nodes from DAC and, if <paramref name="includeFromInheritanceChain"/> is <c>true</c>, its base DACs.
		/// </summary>
		/// <param name="dac">The DAC to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="includeFromInheritanceChain">(Optional) True to include, false to exclude the properties from the inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>The DAC property symbols with nodes from DAC.</returns>
		public static OverridableItemsCollection<DacPropertyInfo> GetDacPropertiesFromDac(this ITypeSymbol dac, PXContext pxContext,
																						  bool includeFromInheritanceChain = true,
																						  CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (!dac.IsDAC(pxContext))
				return new OverridableItemsCollection<DacPropertyInfo>();

			int estimatedCapacity = dac.GetTypeMembers().Length;
			var propertiesByName = new OverridableItemsCollection<DacPropertyInfo>(estimatedCapacity);
			var dacProperties = GetRawPropertiesFromDacImpl(dac, pxContext, includeFromInheritanceChain, cancellation);

			propertiesByName.AddRangeWithDeclarationOrder(dacProperties, startingOrder: 0, 
														  (rawData, order) => new DacPropertyInfo(rawData.Node, rawData.Symbol, order));
			return propertiesByName;
		}

		/// <summary>
		/// Get all properties from DAC or DAC extension and its base DACs and base DAC extensions (extended via Acumatica Customization).
		/// </summary>
		/// <param name="dacOrExtension">The DAC or DAC extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>The properties from DAC or DAC extension and base DAC.</returns>
		public static OverridableItemsCollection<DacPropertyInfo> GetPropertiesFromDacOrDacExtensionAndBaseDac(this ITypeSymbol dacOrExtension,
																											   PXContext pxContext,
																											   CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			bool isDac = dacOrExtension.IsDAC(pxContext);

			if (!isDac && !dacOrExtension.IsDacExtension(pxContext))
				return new OverridableItemsCollection<DacPropertyInfo>();

			return isDac
				? dacOrExtension.GetDacPropertiesFromDac(pxContext)
				: dacOrExtension.GetPropertiesFromDacExtensionAndBaseDac(pxContext);
		}

		/// <summary>
		/// Get DAC properties from the DAC extension and its base DAC.
		/// </summary>
		/// <param name="dacExtension">The DAC extension to act on</param>
		/// <param name="pxContext">Context</param>
		/// <returns/>
		public static OverridableItemsCollection<DacPropertyInfo> GetPropertiesFromDacExtensionAndBaseDac(this ITypeSymbol dacExtension, PXContext pxContext,
																										  CancellationToken cancellation = default)
		{
			dacExtension.ThrowOnNull(nameof(dacExtension));
			pxContext.ThrowOnNull(nameof(pxContext));

			return GetPropertiesOrFieldsInfoFromDacExtension<DacPropertyInfo>(dacExtension, pxContext, AddPropertiesFromDac, AddPropertiesFromDacExtension);

			//-----------------------Local function----------------------------------------
			int AddPropertiesFromDac(OverridableItemsCollection<DacPropertyInfo> propertiesCollection, ITypeSymbol dac, int startingOrder)
			{
				var rawDacProperties = dac.GetRawPropertiesFromDacImpl(pxContext, includeFromInheritanceChain: true, cancellation);
				return propertiesCollection.AddRangeWithDeclarationOrder(rawDacProperties, startingOrder,
																		 (dacProperty, order) => new DacPropertyInfo(dacProperty.Node, dacProperty.Symbol, order));
			}

			int AddPropertiesFromDacExtension(OverridableItemsCollection<DacPropertyInfo> propertiesCollection, ITypeSymbol dacExt, int startingOrder)
			{
				var rawDacExtensionProperties = GetRawPropertiesFromDacOrDacExtensionImpl(dacExt, pxContext, cancellation);
				return propertiesCollection.AddRangeWithDeclarationOrder(rawDacExtensionProperties, startingOrder,
																		  (dacProperty, order) => new DacPropertyInfo(dacProperty.Node, dacProperty.Symbol, order));
			}
		}

		private static IEnumerable<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> GetRawPropertiesFromDacImpl(this ITypeSymbol dac, PXContext pxContext,
																														 bool includeFromInheritanceChain,
																														 CancellationToken cancellation)
		{
			if (includeFromInheritanceChain)
			{
				return dac.GetDacWithBaseTypes()
						  .Reverse()
						  .SelectMany(baseDac => baseDac.GetRawPropertiesFromDacOrDacExtensionImpl(pxContext, cancellation));
			}
			else
			{
				return dac.GetRawPropertiesFromDacOrDacExtensionImpl(pxContext, cancellation);
			}
		}

		private static IEnumerable<(PropertyDeclarationSyntax Node, IPropertySymbol Symbol)> GetRawPropertiesFromDacOrDacExtensionImpl(this ITypeSymbol dacOrExtension,
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

		private static OverridableItemsCollection<TInfo> GetPropertiesOrFieldsInfoFromDacExtension<TInfo>(ITypeSymbol dacExtension, PXContext pxContext,
																		AddDacPropertyInfoWithOrderDelegate<TInfo> addDacPropertyInfoWithOrder,
																		AddDacPropertyInfoWithOrderDelegate<TInfo> addDacExtensionPropertyInfoWithOrder)
		where TInfo : IOverridableItem<TInfo>
		{
			if (!dacExtension.IsDacExtension(pxContext))
				return new OverridableItemsCollection<TInfo>();

			var dacType = dacExtension.GetDacFromDacExtension(pxContext);

			if (dacType == null)
				return new OverridableItemsCollection<TInfo>();

			var allExtensionsFromBaseToDerived = dacExtension.GetDacExtensionWithBaseExtensions(pxContext, SortDirection.Ascending,
																								includeDac: false);
			if (allExtensionsFromBaseToDerived.IsNullOrEmpty())
				return new OverridableItemsCollection<TInfo>();

			int estimatedCapacity = dacType.GetTypeMembers().Length;
			var propertiesByName = new OverridableItemsCollection<TInfo>(estimatedCapacity);
			int declarationOrder = addDacPropertyInfoWithOrder(propertiesByName, dacType, startingOrder: 0);

			foreach (ITypeSymbol extension in allExtensionsFromBaseToDerived)
			{
				declarationOrder = addDacExtensionPropertyInfoWithOrder(propertiesByName, extension, declarationOrder);
			}

			return propertiesByName;
		}
	}
}
