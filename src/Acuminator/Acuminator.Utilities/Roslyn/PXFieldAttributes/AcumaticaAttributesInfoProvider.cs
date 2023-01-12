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
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class AcumaticaAttributesInfoProvider
	{
		private const int MaxRecursionDepth = 50;

		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _dynamicAggregateAttribute;
		private readonly INamedTypeSymbol _aggregateAttribute;

		public PXContext PxContext { get; }

		public AcumaticaAttributesInfoProvider(PXContext pxContext)
		{
			PxContext = pxContext.CheckIfNull(nameof(pxContext));

			_eventSubscriberAttribute = PxContext.AttributeTypes.PXEventSubscriberAttribute;
			_dynamicAggregateAttribute = PxContext.AttributeTypes.PXDynamicAggregateAttribute;
			_aggregateAttribute = PxContext.AttributeTypes.PXAggregateAttribute;
		}

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type. If non Acumatica attributes are passed then <c>flase</c> is returned.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="typeToCheck">The base attribute type to check.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="typeToCheck"/>, false if not.
		/// </returns>
		public bool IsAttributeDerivedFromClass(ITypeSymbol attributeType, ITypeSymbol typeToCheck)
		{
			attributeType.ThrowOnNull(nameof(attributeType));
			typeToCheck.ThrowOnNull(nameof(typeToCheck));

			if (!attributeType.InheritsFromOrEquals(_eventSubscriberAttribute) || !typeToCheck.InheritsFromOrEquals(_eventSubscriberAttribute))
				return false;

			return IsAttributeDerivedFromClass(attributeType, typeToCheck, recursionDepth: 0);
		}

		private bool IsAttributeDerivedFromClass(ITypeSymbol attributeType, ITypeSymbol typeToCheck, int recursionDepth)
		{
			if (attributeType.InheritsFromOrEquals(typeToCheck))
				return true;

			if (recursionDepth > MaxRecursionDepth)
				return false;
			 
			if (IsAggregatorAttribute(attributeType))
			{
				return attributeType.GetAllAttributesDefinedOnThisAndBaseTypes()
									.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute))
									.Any(attribute => IsAttributeDerivedFromClass(attribute, typeToCheck, recursionDepth + 1));
			}

			return false;
		}

		/// <summary>
		/// Get the flattened collection of Acumatica attributes defined by the <paramref name="attributeType"/> including attributes on aggregates.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="includeBaseTypes">(Optional) True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public IEnumerable<ITypeSymbol> GetFlattenedAcumaticaAttributes(ITypeSymbol? attributeType, bool includeBaseTypes = false)
		{
			if (attributeType == null || attributeType.Equals(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			var results = includeBaseTypes
				? baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(_eventSubscriberAttribute)).ToHashSet()
				: new HashSet<ITypeSymbol>() { attributeType };

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(_aggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(_dynamicAggregateAttribute);
			if (!isAggregateAttribute)
				return results;

			var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(attributeType);

			foreach (var attribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
			{
				CollectAggregatedAttributes(results, attribute, includeBaseTypes, recursionDepth: 0);
			}

			return results;
		}

		private void CollectAggregatedAttributes(HashSet<ITypeSymbol> results, ITypeSymbol aggregatedAttribute, bool includeBaseTypes, int recursionDepth)
		{
			results.Add(aggregatedAttribute);

			if (recursionDepth > MaxRecursionDepth)
				return;

			if (includeBaseTypes)
			{
				var baseAcumaticaAttributeTypes = aggregatedAttribute.GetBaseTypes()
																	 .TakeWhile(baseType => !baseType.Equals(_eventSubscriberAttribute));

				foreach (ITypeSymbol baseAcumaticaAttribute in baseAcumaticaAttributeTypes)
					results.Add(baseAcumaticaAttribute!);
			}

			if (IsAggregatorAttribute(aggregatedAttribute))
			{
				var allDeclaredAcumaticaAttributesOnClassHierarchy = GetAllDeclaredAcumaticaAttributesOnClassHierarchy(aggregatedAttribute);

				foreach (var attribute in allDeclaredAcumaticaAttributesOnClassHierarchy)
				{
					CollectAggregatedAttributes(results, attribute, includeBaseTypes, recursionDepth + 1);
				}
			}
		}

		private IEnumerable<ITypeSymbol> GetAllDeclaredAcumaticaAttributesOnClassHierarchy(ITypeSymbol type) =>
			type.GetAllAttributesDefinedOnThisAndBaseTypes()
				.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute));

		private bool IsAggregatorAttribute(ITypeSymbol attributeType) =>
			attributeType.InheritsFromOrEquals(_aggregateAttribute) || attributeType.InheritsFromOrEquals(_dynamicAggregateAttribute);
	}
}