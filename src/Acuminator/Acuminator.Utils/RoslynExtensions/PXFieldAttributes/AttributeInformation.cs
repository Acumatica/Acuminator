using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class AttributeInformation
	{
		private readonly PXContext _context;
		
		public AttributeInformation(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;
		}

		public IEnumerable<ITypeSymbol> AttributesListDerivedFromClass(ITypeSymbol attributeSymbol, bool expand = false)
		{
			HashSet<ITypeSymbol> results = new HashSet<ITypeSymbol>();

			results.Add(attributeSymbol);

			if (expand)
			{
				foreach (var type in attributeSymbol.GetBaseTypesAndThis())
				{
					if (!type.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
						break;

					results.Add(type);
				}
			}

			var aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
			var dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;

			if (attributeSymbol.InheritsFromOrEquals(aggregateAttribute) || attributeSymbol.InheritsFromOrEquals(dynamicAggregateAttribute))
			{
				var allAttributes = attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes();
				foreach (var attribute in allAttributes)
				{
					if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute)) 
						continue;

					results.Add(attribute);
					VisitAggregateAttribute(attribute, 10);
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
				return;
			}
		}

		public bool AttributeDerivedFromClass(ITypeSymbol attributeSymbol, ITypeSymbol type)
		{
			if (attributeSymbol.InheritsFromOrEquals(type))
				return true;

			var aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
			var dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;

			if (attributeSymbol.InheritsFromOrEquals( aggregateAttribute) || attributeSymbol.InheritsFromOrEquals(dynamicAggregateAttribute))
			{
				var allAttributes = attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes();
				foreach (var attribute in allAttributes)
				{
					if (!attribute.GetBaseTypes().Contains(_context.AttributeTypes.PXEventSubscriberAttribute))
						continue;

					var result = VisitAggregateAttribute(attribute,10);

					if (result)
						return result;
				}
			}
			return false;

			bool VisitAggregateAttribute(ITypeSymbol _attributeSymbol,int depth)
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

						var result = VisitAggregateAttribute(attribute,depth-1);

						if (result)
							return result;
					}
				}
				return false;
			}
		}

		public bool IsBoundAttribute(ITypeSymbol attributeSymbol)
		{
			//var dbFieldAttribute = _context.FieldAttributes.PXDBFieldAttribute;
			var parentSymbols = AttributesListDerivedFromClass(attributeSymbol, true);
			var boundTypes = new FieldAttributesRegister(_context).BoundFieldAttributes;

			foreach (var symbol in parentSymbols)
			{
				if(boundTypes.Contains(symbol))
					return true;
			}
			return false;
		}
		
		public bool ContainsBoundAttributes(IEnumerable<ITypeSymbol> attributesSymbols)
		{
			return attributesSymbols.Any(IsBoundAttribute);
		}

	}
}