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
	/// Information about the Acumatica DAC field type attributes.
	/// </summary>
	public class FieldTypeAttributesRegister
	{
		private readonly PXContext _pxContext;

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> UnboundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> BoundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableArray<ITypeSymbol> AttributesCalcedOnDbSide { get; }

		public ImmutableHashSet<ITypeSymbol> AllDacFieldTypeAttributes { get; }

		public ImmutableArray<MixedDbBoundnessAttributeInfo> SortedAttributesContainingIsDBField { get; }

		public FieldTypeAttributesRegister(PXContext pxContext)
		{
			_pxContext = pxContext.CheckIfNull(nameof(pxContext));
			UnboundDacFieldTypeAttributesWithFieldType = GetUnboundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext).ToImmutableDictionary();
			BoundDacFieldTypeAttributesWithFieldType = GetBoundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext).ToImmutableDictionary();

			var attributesCalcedOnDbSide = GetAttributesCalcedOnDbSide(_pxContext);
			AttributesCalcedOnDbSide = attributesCalcedOnDbSide.ToImmutableArray();

			AllDacFieldTypeAttributes = UnboundDacFieldTypeAttributesWithFieldType.Keys
										.Concat(BoundDacFieldTypeAttributesWithFieldType.Keys)
										.Concat(attributesCalcedOnDbSide)
										.ToImmutableHashSet();

			SortedAttributesContainingIsDBField = GetDacFieldTypeAttributesContainingIsDBField(_pxContext)
													.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
													.ToImmutableArray();
		}

		public IReadOnlyCollection<FieldTypeAttributeInfo> GetDacFieldTypeAttributeInfos(ITypeSymbol attributeSymbol)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));

			var flattenedAttributes = attributeSymbol.GetThisAndAllAggregatedAttributes(_pxContext, includeBaseTypes: true);

			if (flattenedAttributes.Count == 0)
				return Array.Empty<FieldTypeAttributeInfo>();

			List<FieldTypeAttributeInfo> typeAttributeInfos = new List<FieldTypeAttributeInfo>(capacity: 2);

			foreach (ITypeSymbol attribute in flattenedAttributes)
			{
				FieldTypeAttributeInfo? attributeInfo = GetDacFieldTypeAttributeInfo(attribute);

				if (attributeInfo != null)
				{
					typeAttributeInfos.Add(attributeInfo);
				}
			}

			return typeAttributeInfos;
		}

		private FieldTypeAttributeInfo? GetDacFieldTypeAttributeInfo(ITypeSymbol attribute)
		{
			var firstDacFieldTypeAttribute = attribute.GetBaseTypesAndThis()
													  .FirstOrDefault(type => AllDacFieldTypeAttributes.Contains(type));
			if (firstDacFieldTypeAttribute == null)
				return null;

			if (firstDacFieldTypeAttribute.Equals(_pxContext.FieldAttributes.PXDBScalarAttribute))
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.PXDBScalarAttribute, fieldType: null);
			else if (firstDacFieldTypeAttribute.Equals(_pxContext.FieldAttributes.PXDBCalcedAttribute))
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.PXDBCalcedAttribute, fieldType: null);

			if (BoundDacFieldTypeAttributesWithFieldType.TryGetValue(firstDacFieldTypeAttribute, out var boundFieldType))
			{
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, boundFieldType);
			}

			if (UnboundDacFieldTypeAttributesWithFieldType.TryGetValue(firstDacFieldTypeAttribute, out var unboundFieldType))
			{
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.UnboundTypeAttribute, unboundFieldType);
			}

			throw new InvalidOperationException($"Cannot get DAC field type attribute info for {attribute}");
		}

		private static List<ITypeSymbol> GetAttributesCalcedOnDbSide(PXContext pxContext) =>
			new List<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXDBScalarAttribute,
				pxContext.FieldAttributes.PXDBCalcedAttribute
			};

		private static Dictionary<ITypeSymbol, ITypeSymbol?> GetUnboundDacFieldTypeAttributesWithCorrespondingTypes(PXContext pxContext)
		{
			return new Dictionary<ITypeSymbol, ITypeSymbol?>
			{
				{ pxContext.FieldAttributes.PXLongAttribute,    pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXIntAttribute,     pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXShortAttribute,   pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXStringAttribute,  pxContext.SystemTypes.String.Type! },
				{ pxContext.FieldAttributes.PXByteAttribute,    pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDecimalAttribute, pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDoubleAttribute,  pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXFloatAttribute,   pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDateAttribute,    pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXGuidAttribute,    pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXBoolAttribute,    pxContext.SystemTypes.Bool },
			};
		}

		private static Dictionary<ITypeSymbol, ITypeSymbol?> GetBoundDacFieldTypeAttributesWithCorrespondingTypes(PXContext pxContext)
		{
			var types = new Dictionary<ITypeSymbol, ITypeSymbol?>
			{
				{ pxContext.FieldAttributes.PXDBFieldAttribute,		   null },
				{ pxContext.FieldAttributes.PXDBLongAttribute,         pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBIntAttribute,          pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBShortAttribute,        pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXDBStringAttribute,       pxContext.SystemTypes.String.Type! },
				{ pxContext.FieldAttributes.PXDBByteAttribute,         pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDBDecimalAttribute,      pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDBDoubleAttribute,       pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXDBFloatAttribute,        pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDBDateAttribute,         pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXDBGuidAttribute,         pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXDBBoolAttribute,         pxContext.SystemTypes.Bool },
				{ pxContext.FieldAttributes.PXDBTimestampAttribute,    pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBIdentityAttribute,     pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBLongIdentityAttribute, pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBBinaryAttribute,       pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBUserPasswordAttribute, pxContext.SystemTypes.String.Type! },
				{ pxContext.FieldAttributes.PXDBAttributeAttribute,    pxContext.SystemTypes.StringArray },
				{ pxContext.FieldAttributes.PXDBDataLengthAttribute,   pxContext.SystemTypes.Int64 },
			};

			var packagedIntegerAttribute = pxContext.FieldAttributes.PXDBPackedIntegerArrayAttribute;

			if (packagedIntegerAttribute != null)
			{
				types.Add(packagedIntegerAttribute, pxContext.SystemTypes.UInt16Array);
			}

			return types;
		}

		private static IEnumerable<MixedDbBoundnessAttributeInfo> GetDacFieldTypeAttributesContainingIsDBField(PXContext context) =>
			new List<MixedDbBoundnessAttributeInfo?>()
			{
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.PeriodIDAttribute,						 context.SystemTypes.String.Type, isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.AcctSubAttribute,						 context.SystemTypes.Int32, isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.UnboundAccountAttribute,				 context.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.UnboundCashAccountAttribute,			 context.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.APTranRecognizedInventoryItemAttribute, context.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(context.FieldAttributes.PXEntityAttribute, null, true)
			}
			.Where(attributeTypeWithIsDbFieldValue => attributeTypeWithIsDbFieldValue != null)!;
	}
}