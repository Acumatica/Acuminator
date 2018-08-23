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

		public IEnumerable<ITypeSymbol> AttributesListDerivedFromClass(ITypeSymbol attributeSymbol)
		{
			return null ;
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
					//go in recursuion
					var result = Recursion(attribute,10);

					if (result)
						return result;
				}
			}
			return false;

			bool Recursion(ITypeSymbol _attributeSymbol,int depth)
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
						//go in recursuion
						var result = Recursion(attribute,depth-1);

						if (result)
							return result;
					}
				}
				return false;
			}
		}

		public bool IsBoundAttribute(ITypeSymbol attributeSymbol)
		{
			var dbFieldAttribute = _context.FieldAttributes.PXDBFieldAttribute;
			return AttributeDerivedFromClass(attributeSymbol, dbFieldAttribute);
		}
		
		public bool ContainsBoundAttributes(IEnumerable<ITypeSymbol> attributesSymbols)
		{
			foreach (var attribute in attributesSymbols)
			{
				if (IsBoundAttribute(attribute))
					return true;
			}
			return false;
		}

	}
}