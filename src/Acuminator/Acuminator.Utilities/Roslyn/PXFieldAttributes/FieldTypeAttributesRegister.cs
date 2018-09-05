using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attributes.
	/// </summary>
	public class FieldTypeAttributesRegister
	{
		private readonly PXContext _context;
		private readonly AttributeInformation _attributeInformation;

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol> CorrespondingSimpleTypes { get; }

		public ImmutableHashSet<ITypeSymbol> UnboundTypeAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> BoundTypeAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> AllTypeAttributes { get; }

		public FieldTypeAttributesRegister(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;
			_attributeInformation = new AttributeInformation(_context);
			var unboundFieldAttributes = GetUnboundTypeAttributes(_context);
			UnboundTypeAttributes = unboundFieldAttributes.ToImmutableHashSet();

			var boundFieldAttributes = GetBoundTypeAttributes(_context);
			BoundTypeAttributes = boundFieldAttributes.ToImmutableHashSet();
			AllTypeAttributes = unboundFieldAttributes.Concat(boundFieldAttributes).ToImmutableHashSet();
			CorrespondingSimpleTypes = GetCorrespondingSimpleTypes(_context).ToImmutableDictionary();
		}

		public FieldAttributeInfo GetFieldAttributeInfo(ITypeSymbol attributeSymbol)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));
		
			var expandedAttributesList = _attributeInformation.GetAcumaticaAttributesFullList(attributeSymbol, includeBaseTypes: true).ToList();



			var info = CheckAttributeInheritanceChain(attributeSymbol, expandedAttributesList);

			if (info.HasValue)
				return info.Value;

			var attributesOnHierarchy = expandedAttributesList.SelectMany(a => a.GetAttributes())
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
			var attributeBaseTypesEnum = attributeTypeHierarchy ?? _attributeInformation.GetAcumaticaAttributesFullList( attributeSymbol,true)
																						.ToList();

			ITypeSymbol fieldAttribute = attributeBaseTypesEnum.TakeWhile(a => !a.Equals(_context.FieldAttributes.PXDBScalarAttribute))
															   .FirstOrDefault(a => AllTypeAttributes.Contains(a));

			if (fieldAttribute == null)
				return null;

			bool isBoundField = BoundTypeAttributes.Contains(fieldAttribute);
			return CorrespondingSimpleTypes.TryGetValue(fieldAttribute, out var fieldType)
				? new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType)
				: new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType: null);
		}

		private static HashSet<ITypeSymbol> GetUnboundTypeAttributes(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXLongAttribute,
				pxContext.FieldAttributes.PXIntAttribute,
				pxContext.FieldAttributes.PXShortAttribute,
				pxContext.FieldAttributes.PXStringAttribute,
				pxContext.FieldAttributes.PXByteAttribute,
				pxContext.FieldAttributes.PXDecimalAttribute,
				pxContext.FieldAttributes.PXFloatAttribute,
				pxContext.FieldAttributes.PXDoubleAttribute,
				pxContext.FieldAttributes.PXDateAttribute,
				pxContext.FieldAttributes.PXGuidAttribute,
				pxContext.FieldAttributes.PXBoolAttribute
			};

		private static HashSet<ITypeSymbol> GetBoundTypeAttributes(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXDBFieldAttribute,

				pxContext.FieldAttributes.PXDBLongAttribute,
				pxContext.FieldAttributes.PXDBIntAttribute,
				pxContext.FieldAttributes.PXDBShortAttribute,
				pxContext.FieldAttributes.PXDBStringAttribute,
				pxContext.FieldAttributes.PXDBByteAttribute,
				pxContext.FieldAttributes.PXDBDecimalAttribute,
				pxContext.FieldAttributes.PXDBDoubleAttribute,
				pxContext.FieldAttributes.PXDBFloatAttribute,
				pxContext.FieldAttributes.PXDBDateAttribute,
				pxContext.FieldAttributes.PXDBGuidAttribute,
				pxContext.FieldAttributes.PXDBBoolAttribute,
				pxContext.FieldAttributes.PXDBTimestampAttribute,
				pxContext.FieldAttributes.PXDBIdentityAttribute,
				pxContext.FieldAttributes.PXDBLongIdentityAttribute,
				pxContext.FieldAttributes.PXDBBinaryAttribute,
				pxContext.FieldAttributes.PXDBUserPasswordAttribute,
				pxContext.FieldAttributes.PXDBCalcedAttribute,
				pxContext.FieldAttributes.PXDBAttributeAttribute,
				pxContext.FieldAttributes.PXDBDataLengthAttribute
			};

		private static Dictionary<ITypeSymbol, ITypeSymbol> GetCorrespondingSimpleTypes(PXContext pxContext) =>
			new Dictionary<ITypeSymbol, ITypeSymbol>
			{
				{ pxContext.FieldAttributes.PXLongAttribute, pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXIntAttribute, pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXShortAttribute, pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXStringAttribute, pxContext.SystemTypes.String },
				{ pxContext.FieldAttributes.PXByteAttribute, pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDecimalAttribute, pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDoubleAttribute, pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXFloatAttribute, pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDateAttribute, pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXGuidAttribute, pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXBoolAttribute, pxContext.SystemTypes.Bool },

				{ pxContext.FieldAttributes.PXDBLongAttribute, pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBIntAttribute, pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBShortAttribute, pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXDBStringAttribute, pxContext.SystemTypes.String },
				{ pxContext.FieldAttributes.PXDBByteAttribute, pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDBDecimalAttribute, pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDBDoubleAttribute, pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXDBFloatAttribute, pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDBDateAttribute, pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXDBGuidAttribute, pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXDBBoolAttribute, pxContext.SystemTypes.Bool },
				{ pxContext.FieldAttributes.PXDBTimestampAttribute, pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBIdentityAttribute, pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBLongIdentityAttribute, pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBBinaryAttribute, pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBUserPasswordAttribute, pxContext.SystemTypes.String },
				{ pxContext.FieldAttributes.PXDBAttributeAttribute, pxContext.SystemTypes.StringArray },
				{ pxContext.FieldAttributes.PXDBDataLengthAttribute, pxContext.SystemTypes.Int64 },
			};
	}
}
