#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;


namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to retrieve info about Acumatica attributes and their relationship with each other.
	/// </summary>
	/// <remarks>
	/// By Acumatica attribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public static class AcumaticaAttributesInfoProvider
	{
		private const int MaxRecursionDepth = 50;

		/// <summary>
		/// Check if <paramref name="attributeType"/> is Acumatica attribute. Acumatica attributes are PXEventSubscriberAttribute attribute and all attributes derived from it.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if is an Acumatica attribute, false if not.
		/// </returns>
		public static bool IsAcumaticaAttribute(this ITypeSymbol attributeType, PXContext pxContext) =>
			pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXEventSubscriberAttribute.Equals(attributeType) ||
			attributeType.IsDerivedFromPXEventSubscriberAttribute(pxContext);

		/// <summary>
		/// Check if <paramref name="attributeType"/> is derived from PXEventSubscriberAttribute attribute (but is not PXEventSubscriberAttribute itself)
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if derived from PXEventSubscriberAttribute, false if not.
		/// </returns>
		public static bool IsDerivedFromPXEventSubscriberAttribute(this ITypeSymbol attributeType, PXContext pxContext) =>
			attributeType.InheritsFrom(pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXEventSubscriberAttribute);

		/// <summary>
		/// Check if <paramref name="attributeType"/> is an Acumatica aggregator attribute.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if is an Acumatica aggregator attribute, false if not.
		/// </returns>
		public static bool IsAggregatorAttribute(this ITypeSymbol attributeType, PXContext pxContext) =>
			attributeType.InheritsFromOrEquals(pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXAggregateAttribute) || 
			attributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXDynamicAggregateAttribute);

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type. If non Acumatica attributes are passed then <c>flase</c> is returned.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="baseAttributeTypeToCheck">The base attribute type to check.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="baseAttributeTypeToCheck"/>, false if not.
		/// </returns>
		public static bool IsDerivedFromAttribute(this ITypeSymbol attributeType, ITypeSymbol baseAttributeTypeToCheck, PXContext pxContext)
		{
			if (!IsAcumaticaAttribute(attributeType, pxContext) || !IsAcumaticaAttribute(baseAttributeTypeToCheck, pxContext))
				return false;

			return IsDerivedFromAttribute(attributeType, baseAttributeTypeToCheck, pxContext, recursionDepth: 0);
		}

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type. <br/>
		/// This is an internal unsafe version which for performance reasons doesn't check input types for being Acumatica attributes.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="baseAttributeTypeToCheck">The base attribute type to check.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="baseAttributeTypeToCheck"/>, false if not.
		/// </returns>
		internal static bool IsDerivedFromAttributeUnsafe(this ITypeSymbol attributeType, ITypeSymbol baseAttributeTypeToCheck, PXContext pxContext) =>
			 IsDerivedFromAttribute(attributeType, baseAttributeTypeToCheck, pxContext.CheckIfNull(nameof(pxContext)), recursionDepth: 0);

		private static bool IsDerivedFromAttribute(ITypeSymbol attributeType, ITypeSymbol baseAttributeTypeToCheck, PXContext pxContext, int recursionDepth)
		{
			if (attributeType.InheritsFromOrEquals(baseAttributeTypeToCheck))
				return true;

			if (recursionDepth > MaxRecursionDepth)
				return false;
			 
			if (attributeType.IsAggregatorAttribute(pxContext))
			{
				var aggregatedAcumaticaAttributes = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(attributeType, pxContext);

				foreach (var aggregatedAttribute in aggregatedAcumaticaAttributes)
				{
					if (IsDerivedFromAttribute(aggregatedAttribute, baseAttributeTypeToCheck, pxContext, recursionDepth + 1))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get the flattened collection of Acumatica attributes defined by the <paramref name="attributeType"/> including attributes on aggregates and <paramref name="attributeType"/> itself.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="includeBaseTypes">True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public static IReadOnlyCollection<ITypeSymbol> GetThisAndAllAggregatedAttributes(this ITypeSymbol? attributeType, PXContext pxContext, bool includeBaseTypes)
		{
			var eventSubscriberAttribute = pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXEventSubscriberAttribute;

			if (attributeType == null || attributeType.Equals(eventSubscriberAttribute))
				return Array.Empty<ITypeSymbol>();

			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(eventSubscriberAttribute))
				return Array.Empty<ITypeSymbol>();

			var results = includeBaseTypes
				? baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(eventSubscriberAttribute)).ToHashSet()
				: new HashSet<ITypeSymbol>() { attributeType };

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(pxContext.AttributeTypes.PXAggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(pxContext.AttributeTypes.PXDynamicAggregateAttribute);
			if (!isAggregateAttribute)
				return results;

			var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(attributeType, pxContext);

			foreach (var attribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
			{
				CollectAggregatedAttributes(results, attribute, pxContext, includeBaseTypes, recursionDepth: 0);
			}

			return results;
		}

		private static void CollectAggregatedAttributes(HashSet<ITypeSymbol> results, ITypeSymbol aggregatedAttribute, PXContext pxContext, 
														bool includeBaseTypes, int recursionDepth)
		{
			if (!results.Add(aggregatedAttribute) || recursionDepth > MaxRecursionDepth)
				return;

			if (includeBaseTypes)
			{
				var baseAcumaticaAttributeTypes = 
					aggregatedAttribute.GetBaseTypes()
									   .TakeWhile(baseType => !baseType.Equals(pxContext.AttributeTypes.PXEventSubscriberAttribute));

				results.AddRange(baseAcumaticaAttributeTypes);					
			}

			if (aggregatedAttribute.IsAggregatorAttribute(pxContext))
			{
				var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(aggregatedAttribute, pxContext);

				foreach (var attribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
				{
					CollectAggregatedAttributes(results, attribute, pxContext, includeBaseTypes, recursionDepth + 1);
				}
			}
		}

		private static IEnumerable<ITypeSymbol> GetAllDeclaredAcumaticaAttributesOnClassHierarchy(ITypeSymbol type, PXContext pxContext) =>
			type.GetAllAttributesDefinedOnThisAndBaseTypes()
				.Where(attribute => attribute.IsDerivedFromPXEventSubscriberAttribute(pxContext));
	}
}