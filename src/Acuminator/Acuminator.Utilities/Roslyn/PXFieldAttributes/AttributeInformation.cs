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
	/// Helper used to retrieve information about the Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class AttributeInformation
	{
		private const int DefaultRecursionDepth = 10;
		private readonly PXContext _context;

		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _dynamicAggregateAttribute;
		private readonly INamedTypeSymbol _aggregateAttribute;

		public ImmutableHashSet<ITypeSymbol> BoundBaseTypes { get; }

		public AttributeInformation(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;

			var boundBaseTypes = GetBoundBaseTypes(_context);
			BoundBaseTypes = boundBaseTypes.ToImmutableHashSet();

			_eventSubscriberAttribute = _context.AttributeTypes.PXEventSubscriberAttribute;
			_dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;
			_aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
		}

		private static HashSet<ITypeSymbol> GetBoundBaseTypes(PXContext context) =>
			new HashSet<ITypeSymbol>
			{
				context.FieldAttributes.PXDBFieldAttribute,
				context.FieldAttributes.PXDBCalcedAttribute,
				context.FieldAttributes.PXDBDataLengthAttribute,
			};

		/// <summary>
		/// Get the collection of Acumatica attributes defined by the <paramref name="attributeType"/> including attributes on aggregates.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="includeBaseTypes">(Optional) True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public IEnumerable<ITypeSymbol> GetAcumaticaAttributesFullList(ITypeSymbol attributeType, bool includeBaseTypes = false)
		{
			if (attributeType == null || attributeType.Equals(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			HashSet<ITypeSymbol> results;

			if (includeBaseTypes)
			{
				results = baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(_eventSubscriberAttribute))
													 .ToHashSet();
			}
			else
			{
				results = new HashSet<ITypeSymbol>() { attributeType };
			}

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(_aggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(_dynamicAggregateAttribute);

			if (isAggregateAttribute)
			{
				var allAcumaticaAttributes = attributeType.GetAllAttributesDefinedOnThisAndBaseTypes()
														  .Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute));

				foreach (var attribute in allAcumaticaAttributes)
				{				
					CollectAggregatedAttribute(attribute, DefaultRecursionDepth);
				}
			}

			return results;


			void CollectAggregatedAttribute(ITypeSymbol aggregatedAttribute, int depth)
			{
				results.Add(aggregatedAttribute);

				if (depth < 0)
					return;

				if (includeBaseTypes)
				{
					aggregatedAttribute.GetBaseTypes()
									   .TakeWhile(baseType => !baseType.Equals(_eventSubscriberAttribute))
									   .ForEach(baseType => results.Add(baseType));
				}

				if (IsAggregatorAttribute(aggregatedAttribute))
				{
					var allAcumaticaAttributes = aggregatedAttribute.GetAllAttributesDefinedOnThisAndBaseTypes()
																	.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute));

					foreach (var attribute in allAcumaticaAttributes)
					{
						CollectAggregatedAttribute(attribute, depth - 1);
					}
				}
			}
		}

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type. The exception will be thrown if non Acumatica attributes are passed.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown when one or more arguments is non Acumatica attribute.</exception>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="typeToCheck">The base attribute type to check.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="typeToCheck"/>, false if not.
		/// </returns>
		public bool IsAttributeDerivedFromClass(ITypeSymbol attributeType, ITypeSymbol typeToCheck)
		{
			attributeType.ThrowOnNull(nameof(attributeType));
			typeToCheck.ThrowOnNull(nameof(typeToCheck));

			if (!attributeType.InheritsFromOrEquals(_eventSubscriberAttribute))
				throw new ArgumentException("Attribute must be derived from PXEventSubscriber", nameof(attributeType));
			if (!typeToCheck.InheritsFromOrEquals(_eventSubscriberAttribute))
				throw new ArgumentException("Attribute must be derived from PXEventSubscriber", nameof(typeToCheck));
			
			return IsAttributeDerivedFromClassInternal(attributeType, typeToCheck);
		}

		/// <summary>
		/// Query if Acumatica attribute is bound.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <returns/>
		public bool IsBoundAttribute(ITypeSymbol attributeType)
		{
			attributeType.ThrowOnNull(nameof(attributeType));

			if (!attributeType.InheritsFromOrEquals(_eventSubscriberAttribute))
				return false;

			return BoundBaseTypes.Any(boundBaseType => IsAttributeDerivedFromClassInternal(attributeType, boundBaseType));		
		}

		/// <summary>
		/// Query if collection of attributes contains bound attribute.
		/// </summary>
		/// <param name="attributeTypes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsBoundAttributes(IEnumerable<ITypeSymbol> attributeTypes)
		{
			return attributeTypes.Any(IsBoundAttribute);
		}

		private bool IsAttributeDerivedFromClassInternal(ITypeSymbol attributeType, ITypeSymbol typeToCheck, int depth = DefaultRecursionDepth)
		{
			if (depth < 0)
				return false;
			else if (attributeType.InheritsFromOrEquals(typeToCheck))
				return true;

			if (IsAggregatorAttribute(attributeType))
			{
				return attributeType.GetAllAttributesDefinedOnThisAndBaseTypes()
									.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute))
									.Any(attribute => IsAttributeDerivedFromClassInternal(attribute, typeToCheck, depth - 1));
			}

			return false;
		}

		private bool IsAggregatorAttribute(ITypeSymbol attributeType) => 
			attributeType.InheritsFromOrEquals(_aggregateAttribute) || attributeType.InheritsFromOrEquals(_dynamicAggregateAttribute);
	}
}