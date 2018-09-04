using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class AttributeInformation
	{
		private const int DefaultRecursionDepth = 10;
		private readonly PXContext _context;

		public ImmutableHashSet<ITypeSymbol> BoundBaseTypes { get; }

		public AttributeInformation(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;

			var boundBaseTypes = GetBoundBaseTypes(_context);
			BoundBaseTypes = boundBaseTypes.ToImmutableHashSet();
		}

		private static HashSet<ITypeSymbol> GetBoundBaseTypes(PXContext context) =>
			new HashSet<ITypeSymbol>
			{
				context.FieldAttributes.PXDBFieldAttribute,
				context.FieldAttributes.PXDBCalcedAttribute,
				context.FieldAttributes.PXDBDataLengthAttribute,
			};

		public HashSet<ITypeSymbol> AttributesListDerivedFromClass(ITypeSymbol attributeType, bool expand = false)
		{
			HashSet<ITypeSymbol> results = new HashSet<ITypeSymbol>();
			results.Add(attributeType);

			if (expand)
			{
				var baseAcumaticaAttributeTypes = GetBaseAcumaticaAttributeTypes(attributeType);
				baseAcumaticaAttributeTypes.ForEach(aType => results.Add(aType));
			}

			var aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
			var dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;

			if (attributeType.InheritsFromOrEquals(aggregateAttribute) || attributeType.InheritsFromOrEquals(dynamicAggregateAttribute))
			{
				var allAttributes = attributeType.GetAllAttributesDefinedOnThisAndBaseTypes();

				foreach (var attribute in allAttributes)
				{
					if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
						continue;

					results.Add(attribute);
					VisitAggregateAttribute(attribute, DefaultRecursionDepth);
				}
			}

			return results;

			void VisitAggregateAttribute(ITypeSymbol _attributeSymbol, int depth)
			{
				if (depth < 0)
					return;

				if (expand)
				{
					foreach (var type in _attributeSymbol.GetBaseTypesAndThis())
					{
						if (!type.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
							break;

						results.Add(type);
					}
				}

				if (_attributeSymbol.InheritsFromOrEquals(aggregateAttribute) || _attributeSymbol.InheritsFromOrEquals(dynamicAggregateAttribute))
				{
					var allAttributes = _attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes();
					foreach (var attribute in allAttributes)
					{
						if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
							continue;

						results.Add(attribute);
						VisitAggregateAttribute(attribute, depth - 1);
					}
				}
			}
		}

		public bool AttributeDerivedFromClass(ITypeSymbol attributeSymbol, ITypeSymbol type)
		{
			if (attributeSymbol.InheritsFromOrEquals(type))
				return true;

			var aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
			var dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;

			if (attributeSymbol.InheritsFromOrEquals(aggregateAttribute) || attributeSymbol.InheritsFromOrEquals(dynamicAggregateAttribute))
			{
				var allAttributes = attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes();

				foreach (var attribute in allAttributes)
				{
					if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
						continue;

					var result = VisitAggregateAttribute(attribute, DefaultRecursionDepth);

					if (result)
						return result;
				}
			}

			return false;

			bool VisitAggregateAttribute(ITypeSymbol _attributeSymbol, int depth)
			{
				if (depth < 0)
					return false;

				if (_attributeSymbol.InheritsFromOrEquals(type))
					return true;

				if (_attributeSymbol.InheritsFromOrEquals(aggregateAttribute) || _attributeSymbol.InheritsFromOrEquals(dynamicAggregateAttribute))
				{
					var allAttributes = _attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes();

					foreach (var attribute in allAttributes)
					{
						if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
							continue;

						var result = VisitAggregateAttribute(attribute, depth - 1);

						if (result)
							return result;
					}
				}

				return false;
			}
		}

		public bool IsBoundAttribute(ITypeSymbol attributeSymbol)
		{
			foreach (var baseType in BoundBaseTypes)
			{
				if (AttributeDerivedFromClass(attributeSymbol, baseType))
					return true;
			}
			return false;
		}

		public bool ContainsBoundAttributes(IEnumerable<ITypeSymbol> attributesSymbols)
		{
			return attributesSymbols.Any(IsBoundAttribute);
		}

		private IEnumerable<ITypeSymbol> GetBaseAcumaticaAttributeTypes(ITypeSymbol attributeType)
		{
			bool isAcumaticaAttribute = false;
			List<ITypeSymbol> baseAcumaticaAttributeTypes = new List<ITypeSymbol>(capacity: 4);

			foreach (ITypeSymbol baseAttributeType in attributeType.GetBaseTypesAndThis())
			{
				if (baseAttributeType.Equals(_context.AttributeTypes.PXEventSubscriberAttribute))
				{
					isAcumaticaAttribute = true;
					break;
				}

				baseAcumaticaAttributeTypes.Add(baseAttributeType);
			}

			return isAcumaticaAttribute 
				? baseAcumaticaAttributeTypes
				: Enumerable.Empty<ITypeSymbol>();
		}
	}
}