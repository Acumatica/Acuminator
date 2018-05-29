using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using PX.Data;
using Acuminator.Analyzers;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class FieldAttributesRegister
	{
		private readonly PXContext context;

		public ImmutableDictionary<ITypeSymbol, INamedTypeSymbol> CorrespondingSimpleTypes { get; }

		public ImmutableHashSet<INamedTypeSymbol> UnboundFieldAttributes { get; }
		public ImmutableHashSet<INamedTypeSymbol> BoundFieldAttributes { get; }
		public ImmutableHashSet<INamedTypeSymbol> AllFieldAttributes { get; }

		public FieldAttributesRegister(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			context = pxContext;
			var unboundFieldAttributes = GetUnboundFieldAttributes(context);
			UnboundFieldAttributes = unboundFieldAttributes.ToImmutableHashSet();

			var boundFieldAttributes = GetBoundFieldAttributes(context);
			BoundFieldAttributes = boundFieldAttributes.ToImmutableHashSet();
			AllFieldAttributes = unboundFieldAttributes.Concat(boundFieldAttributes).ToImmutableHashSet();
			CorrespondingSimpleTypes = GetCorrespondingSimpleTypes(context).ToImmutableDictionary();
		}

		public FieldAttributeInfo GetFieldAttributeInfo(ITypeSymbol attributeSymbol)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));

			List<ITypeSymbol> attributeTypeHierarchy = attributeSymbol.GetBaseTypesAndThis().ToList();        
			var info = CheckAttributeInheritanceChain(attributeSymbol, attributeTypeHierarchy);

			if (info.HasValue)
				return info.Value;

			var attributesOnHierarchy = attributeTypeHierarchy.SelectMany(a => a.GetAttributes())
															  .Select(a => a.AttributeClass);
														 
			foreach (ITypeSymbol attribute in attributesOnHierarchy)
			{
				info = CheckAttributeInheritanceChain(attribute);

				if (info.HasValue)
					return info.Value;
			}

			return new FieldAttributeInfo(isFieldAttribute: false, isBoundField: false, fieldType: null);
		}

		private FieldAttributeInfo? CheckAttributeInheritanceChain(ITypeSymbol attributeSymbol, List<ITypeSymbol> attributeTypeHierarchy = null)
		{
			if (!attributeSymbol.ImplementsInterface(context.FieldAttributes.IPXFieldUpdatingSubscriber))
				return null;

			var attributeBaseTypesEnum = attributeTypeHierarchy ?? attributeSymbol.GetBaseTypesAndThis();
			ITypeSymbol fieldAttribute = attributeBaseTypesEnum.FirstOrDefault(attr => AllFieldAttributes.Contains(attr));

			if (fieldAttribute != null)
			{
				bool isBoundField = BoundFieldAttributes.Contains(fieldAttribute);
				return CorrespondingSimpleTypes.TryGetValue(fieldAttribute, out var fieldType)
					? new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType)
					: new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType: null);
			}

			return null;
		}

		private static HashSet<INamedTypeSymbol> GetUnboundFieldAttributes(PXContext pxContext)
		{
			return new HashSet<INamedTypeSymbol>
			{
				pxContext.FieldAttributes.PXLongAttribute,
				pxContext.FieldAttributes.PXIntAttribute,
				pxContext.FieldAttributes.PXShortAttribute,
				pxContext.FieldAttributes.PXStringAttribute,
				pxContext.FieldAttributes.PXByteAttribute,
				pxContext.FieldAttributes.PXDecimalAttribute,
				pxContext.FieldAttributes.PXDoubleAttribute,
				pxContext.FieldAttributes.PXDateAttribute,
				pxContext.FieldAttributes.PXGuidAttribute,
				pxContext.FieldAttributes.PXBoolAttribute
			};
		}

		private static HashSet<INamedTypeSymbol> GetBoundFieldAttributes(PXContext pxContext)
		{
			return new HashSet<INamedTypeSymbol>
			{
				pxContext.FieldAttributes.PXDBFieldAttribute,

				pxContext.FieldAttributes.PXDBLongAttribute,
				pxContext.FieldAttributes.PXDBIntAttribute,
				pxContext.FieldAttributes.PXDBShortAttribute,
				pxContext.FieldAttributes.PXDBStringAttribute,
				pxContext.FieldAttributes.PXDBByteAttribute,
				pxContext.FieldAttributes.PXDBDecimalAttribute,
				pxContext.FieldAttributes.PXDBDoubleAttribute,
				pxContext.FieldAttributes.PXDBDateAttribute,
				pxContext.FieldAttributes.PXDBGuidAttribute,
				pxContext.FieldAttributes.PXDBBoolAttribute
			};
		}

		private static Dictionary<ITypeSymbol, INamedTypeSymbol> GetCorrespondingSimpleTypes(PXContext pxContext)
		{
			return new Dictionary<ITypeSymbol, INamedTypeSymbol>
			{
				{ pxContext.FieldAttributes.PXLongAttribute, pxContext.Int64 },
				{ pxContext.FieldAttributes.PXIntAttribute, pxContext.Int32 },
				{ pxContext.FieldAttributes.PXShortAttribute, pxContext.Int16 },
				{ pxContext.FieldAttributes.PXStringAttribute, pxContext.String },
				{ pxContext.FieldAttributes.PXByteAttribute, pxContext.Byte },
				{ pxContext.FieldAttributes.PXDecimalAttribute, pxContext.Decimal },
				{ pxContext.FieldAttributes.PXDoubleAttribute, pxContext.Double },
				{ pxContext.FieldAttributes.PXDateAttribute, pxContext.DateTime },
				{ pxContext.FieldAttributes.PXGuidAttribute, pxContext.Guid },
				{ pxContext.FieldAttributes.PXBoolAttribute, pxContext.Bool },

				{ pxContext.FieldAttributes.PXDBLongAttribute, pxContext.Int64 },
				{ pxContext.FieldAttributes.PXDBIntAttribute, pxContext.Int32 },
				{ pxContext.FieldAttributes.PXDBShortAttribute, pxContext.Int16 },
				{ pxContext.FieldAttributes.PXDBStringAttribute, pxContext.String },
				{ pxContext.FieldAttributes.PXDBByteAttribute, pxContext.Byte },
				{ pxContext.FieldAttributes.PXDBDecimalAttribute, pxContext.Decimal },
				{ pxContext.FieldAttributes.PXDBDoubleAttribute, pxContext.Double },
				{ pxContext.FieldAttributes.PXDBDateAttribute, pxContext.DateTime },
				{ pxContext.FieldAttributes.PXDBGuidAttribute, pxContext.Guid },
				{ pxContext.FieldAttributes.PXDBBoolAttribute, pxContext.Bool }
			};
		}
	}
}
