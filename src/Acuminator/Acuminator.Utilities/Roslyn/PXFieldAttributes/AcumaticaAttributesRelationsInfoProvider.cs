#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	public static class AcumaticaAttributesRelationsInfoProvider
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
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type or aggregates it.<br/>
		/// If non Acumatica attributes are passed then <c>false</c> is returned.
		/// </summary>
		/// <remarks>
		/// This check imitates Acumatica runtime processing of Acumatica attributes which can be dividen into two groups:<br/>
		/// - Normal attributes that contain some shared functionality (usually in a form of event subscription) which can be reused between Acumatica graphs.<br/>
		/// - Aggregate attributes that besides their own functionality also collect all attributes declared on them and merge logic from these aggregated attributes.<br/>
		/// Since aggregate attributes are also Acumatica attributes they can also be aggregated by other aggregate attributes although such complex scenarios are rare.<br/>
		/// Thus, the resolution of all attributes is a recursive process. 
		/// </remarks>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="baseAttributeTypeToCheck">The base attribute type to check.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="baseAttributeTypeToCheck"/>, false if not.
		/// </returns>
		public static bool IsDerivedFromOrAggregatesAttribute(this ITypeSymbol attributeType, ITypeSymbol baseAttributeTypeToCheck, PXContext pxContext)
		{
			if (!IsAcumaticaAttribute(attributeType, pxContext) || !IsAcumaticaAttribute(baseAttributeTypeToCheck, pxContext))
				return false;

			return IsDerivedFromAttribute(attributeType, baseAttributeTypeToCheck, pxContext, recursionDepth: 0);
		}

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type or aggregates it.<br/>
		/// This is an internal unsafe version which for performance reasons doesn't check input types for being Acumatica attributes.
		/// </summary>
		///  <remarks>
		/// This check imitates Acumatica runtime processing of Acumatica attributes which can be dividen into two groups:<br/>
		/// - Normal attributes that contain some shared functionality (usually in a form of event subscription) which can be reused between Acumatica graphs.<br/>
		/// - Aggregate attributes that besides their own functionality also collect all attributes declared on them and merge logic from these aggregated attributes.<br/>
		/// Since aggregate attributes are also Acumatica attributes they can also be aggregated by other aggregate attributes although such complex scenarios are rare.<br/>
		/// Thus, the resolution of all attributes is a recursive process.
		/// </remarks>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="baseAttributeTypeToCheck">The base attribute type to check.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="baseAttributeTypeToCheck"/>, false if not.
		/// </returns>
		internal static bool IsDerivedFromOrAggregatesAttributeUnsafe(this ITypeSymbol attributeType, ITypeSymbol baseAttributeTypeToCheck, 
																	  PXContext pxContext) =>
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
		///  <remarks>
		/// This check imitates Acumatica runtime processing of Acumatica attributes which can be dividen into two groups:<br/>
		/// - Normal attributes that contain some shared functionality (usually in a form of event subscription) which can be reused between Acumatica graphs.<br/>
		/// - Aggregate attributes that besides their own functionality also collect all attributes declared on them and merge logic from these aggregated attributes.<br/>
		/// Since aggregate attributes are also Acumatica attributes they can also be aggregated by other aggregate attributes although such complex scenarios are rare.<br/>
		/// Thus, the resolution of all attributes is a recursive process.
		/// </remarks>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="includeBaseTypes">True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public static ImmutableHashSet<ITypeSymbol> GetThisAndAllAggregatedAttributes(this ITypeSymbol? attributeType, PXContext pxContext, bool includeBaseTypes)
		{
			var eventSubscriberAttribute = pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXEventSubscriberAttribute;

			if (attributeType == null || attributeType.Equals(eventSubscriberAttribute))
				return ImmutableHashSet<ITypeSymbol>.Empty;

			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(eventSubscriberAttribute))
				return ImmutableHashSet<ITypeSymbol>.Empty;

			INamedTypeSymbol pxAggregateAttribute = pxContext.AttributeTypes.PXAggregateAttribute;
			INamedTypeSymbol pxDynamicAggregateAttribute = pxContext.AttributeTypes.PXDynamicAggregateAttribute;

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(pxAggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(pxDynamicAggregateAttribute);
			if (!isAggregateAttribute)
			{
				return includeBaseTypes
					? baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(eventSubscriberAttribute)).ToImmutableHashSet()
					: ImmutableHashSet.Create(attributeType);
			}

			var results = includeBaseTypes
				? baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(eventSubscriberAttribute)).ToHashSet()
				: new HashSet<ITypeSymbol>() { attributeType };

			var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(attributeType, pxContext);

			foreach (var aggregatedAttribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
			{
				CollectAggregatedAttributes(aggregatedAttribute, recursionDepth: 0);
			}

			return results.ToImmutableHashSet();

			//--------------------------------------------------Local Function-----------------------------------------------------
			void CollectAggregatedAttributes(ITypeSymbol aggregatedAttribute, int recursionDepth)
			{
				if (!results.Add(aggregatedAttribute) || recursionDepth > MaxRecursionDepth)
					return;

				var aggregatedAttributeBaseTypes = aggregatedAttribute.GetBaseTypes().ToList();

				if (includeBaseTypes)
				{
					var aggregatedAttributeBaseAcumaticaAttributeTypes =
						aggregatedAttributeBaseTypes.TakeWhile(baseType => !baseType.Equals(eventSubscriberAttribute));

					results.AddRange(aggregatedAttributeBaseAcumaticaAttributeTypes);
				}

				bool isAggregateOnAggregateAttribute = aggregatedAttributeBaseTypes.Contains(pxAggregateAttribute) ||
													   aggregatedAttributeBaseTypes.Contains(pxDynamicAggregateAttribute);
				if (isAggregateOnAggregateAttribute)
				{
					var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(aggregatedAttribute, pxContext);

					foreach (var attribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
					{
						CollectAggregatedAttributes(attribute, recursionDepth + 1);
					}
				}
			}
		}

		/// <summary>
		/// Get the flattened collection of Acumatica attributes with application data defined by the <paramref name="attributeType"/> including attributes on aggregates 
		/// and <paramref name="attributeType"/> itself.
		/// </summary>
		///  <remarks>
		/// This check imitates Acumatica runtime processing of Acumatica attributes which can be dividen into two groups:<br/>
		/// - Normal attributes that contain some shared functionality (usually in a form of event subscription) which can be reused between Acumatica graphs.<br/>
		/// - Aggregate attributes that besides their own functionality also collect all attributes declared on them and merge logic from these aggregated attributes.<br/>
		/// Since aggregate attributes are also Acumatica attributes they can also be aggregated by other aggregate attributes although such complex scenarios are rare.<br/>
		/// Thus, the resolution of all attributes is a recursive process.
		/// </remarks>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="includeBaseTypes">True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public static ImmutableHashSet<AttributeWithApplication> GetThisAndAllAggregatedAttributesWithApplications(this AttributeData? attributeApplication,
																												   PXContext pxContext, bool includeBaseTypes)
		{
			var eventSubscriberAttribute = pxContext.CheckIfNull(nameof(pxContext)).AttributeTypes.PXEventSubscriberAttribute;

			if (attributeApplication?.AttributeClass == null || attributeApplication.AttributeClass.Equals(eventSubscriberAttribute))
				return ImmutableHashSet<AttributeWithApplication>.Empty;

			var attributeType = attributeApplication.AttributeClass;
			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(eventSubscriberAttribute))
				return ImmutableHashSet<AttributeWithApplication>.Empty;

			var attributeWithApplication = new AttributeWithApplication(attributeApplication);
			INamedTypeSymbol pxAggregateAttribute = pxContext.AttributeTypes.PXAggregateAttribute;
			INamedTypeSymbol pxDynamicAggregateAttribute = pxContext.AttributeTypes.PXDynamicAggregateAttribute;

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(pxAggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(pxDynamicAggregateAttribute);
			if (!isAggregateAttribute)
			{
				// We can suppose that attribute types hierarchy shares the attribute application 
				return includeBaseTypes
					? baseAcumaticaAttributeTypes.TakeWhile(type => !type.Equals(eventSubscriberAttribute))
												 .Select(type => new AttributeWithApplication(attributeApplication, type))
												 .ToImmutableHashSet()
					: ImmutableHashSet.Create(attributeWithApplication);
			}

			var results = includeBaseTypes
				? baseAcumaticaAttributeTypes.TakeWhile(type => !type.Equals(eventSubscriberAttribute))
											 .Select(type => new AttributeWithApplication(attributeApplication, type))
											 .ToHashSet()
				: new HashSet<AttributeWithApplication>() { attributeWithApplication };

			var allDeclaredAcumaticaAttributesApplicationsOnClassHierarchy = 
				attributeType.GetAllAttributesApplicationsDefinedOnThisAndBaseTypes()
							 .Where(application => application.AttributeClass.InheritsFrom(eventSubscriberAttribute));

			foreach (AttributeData aggregatedAttributeApplication in allDeclaredAcumaticaAttributesApplicationsOnClassHierarchy)
			{
				var aggregatedAttributeWithApplication = new AttributeWithApplication(aggregatedAttributeApplication);
				CollectAggregatedAttributeWithApplications(aggregatedAttributeWithApplication, recursionDepth: 0);
			}

			return results.ToImmutableHashSet();

			//--------------------------------------------------Local Function-----------------------------------------------------
			void CollectAggregatedAttributeWithApplications(AttributeWithApplication aggregatedAttributeWithApplication, int recursionDepth)
			{
				if (!results.Add(aggregatedAttributeWithApplication) || recursionDepth > MaxRecursionDepth)
					return;

				var aggregatedAttributeBaseTypes = aggregatedAttributeWithApplication.Type.GetBaseTypes().ToList();

				if (includeBaseTypes)
				{
					var aggregatedAttributeBaseAcumaticaAttributeTypesWithApplications =
						aggregatedAttributeBaseTypes.TakeWhile(baseType => !baseType.Equals(eventSubscriberAttribute))
													.Select(baseType => new AttributeWithApplication(aggregatedAttributeWithApplication.Application, baseType));

					results.AddRange(aggregatedAttributeBaseAcumaticaAttributeTypesWithApplications);
				}

				bool isAggregateOnAggregateAttribute = aggregatedAttributeBaseTypes.Contains(pxAggregateAttribute) ||
													   aggregatedAttributeBaseTypes.Contains(pxDynamicAggregateAttribute);
				if (isAggregateOnAggregateAttribute)
				{
					var allDeclaredAcumaticaAttributesApplicationsOnClassHierarchy =
						aggregatedAttributeWithApplication.Type.GetAllAttributesApplicationsDefinedOnThisAndBaseTypes()
															   .Where(application => application.AttributeClass.InheritsFrom(eventSubscriberAttribute));

					foreach (AttributeData aggregatedOnAggregateAttributeApplication in allDeclaredAcumaticaAttributesApplicationsOnClassHierarchy)
					{
						var aggregatedOnAggregateAttributeWithApplication = new AttributeWithApplication(aggregatedOnAggregateAttributeApplication);
						CollectAggregatedAttributeWithApplications(aggregatedOnAggregateAttributeWithApplication, recursionDepth + 1);
					}
				}
			}
		}

		/// <summary>
		/// Check if <paramref name="attribute"/> type equals to <paramref name="attributeToCheck"/> type or aggregates <paramref name="attributeToCheck"/>.<br/>
		/// No types derived from <paramref name="attributeToCheck"/> are checked, it must be either equal to <paramref name="attribute"/> or directly aggregated on it.
		/// </summary>
		/// <param name="attribute">Type of the attribute.</param>
		/// <param name="attributeToCheck">The attribute type to check.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// True if equals or directly aggregates attribute, false if not.
		/// </returns>
		internal static bool EqualsOrAggregatesAttributeDirectly(this ITypeSymbol attribute, ITypeSymbol attributeToCheck, PXContext pxContext) =>
			EqualsOrAggregatesAttributeDirectly(attribute.CheckIfNull(nameof(attribute)), 
												attributeToCheck.CheckIfNull(nameof(attributeToCheck)), 
												pxContext.CheckIfNull(nameof(pxContext)), recursionDepth: 0);

		private static bool EqualsOrAggregatesAttributeDirectly(ITypeSymbol attribute, ITypeSymbol attributeToCheck, PXContext pxContext, int recursionDepth)
		{
			if (attribute.Equals(attributeToCheck))
				return true;

			if (recursionDepth > MaxRecursionDepth)
				return false;

			if (attribute.IsAggregatorAttribute(pxContext))
			{
				var aggregatedAcumaticaAttributes = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(attribute, pxContext);

				foreach (var aggregatedAttribute in aggregatedAcumaticaAttributes)
				{
					if (EqualsOrAggregatesAttributeDirectly(aggregatedAttribute, attributeToCheck, pxContext, recursionDepth + 1))
						return true;
				}
			}

			return false;
		}

		private static IEnumerable<ITypeSymbol> GetAllDeclaredAcumaticaAttributesOnClassHierarchy(ITypeSymbol type, PXContext pxContext) =>
			type.GetAllAttributesDefinedOnThisAndBaseTypes()
				.Where(attribute => attribute.IsDerivedFromPXEventSubscriberAttribute(pxContext));
	}
}