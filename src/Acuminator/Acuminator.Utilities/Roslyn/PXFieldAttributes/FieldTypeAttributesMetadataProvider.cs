﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Provider of information about the Acumatica DAC field type attributes metadata.
	/// </summary>
	public class FieldTypeAttributesMetadataProvider
	{
		private readonly PXContext _pxContext;

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> UnboundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> BoundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableArray<INamedTypeSymbol> AttributesCalcedOnDbSide { get; }

		public ImmutableHashSet<ITypeSymbol> AllDacFieldTypeAttributes { get; }

		public ImmutableArray<MixedDbBoundnessAttributeInfo> SortedDacFieldTypeAttributesWithMixedDbBoundness { get; }

		public ImmutableArray<INamedTypeSymbol> WellKnownNonDataTypeAttributes { get; }

		private readonly INamedTypeSymbol _pxDBCalcedAttribute;
		private readonly INamedTypeSymbol _pxDBScalarAttribute;
		private readonly INamedTypeSymbol _pxDBFieldAttribute;

		public FieldTypeAttributesMetadataProvider(PXContext pxContext)
		{
			_pxContext = pxContext.CheckIfNull(nameof(pxContext));
			_pxDBScalarAttribute = _pxContext.FieldAttributes.PXDBScalarAttribute;
			_pxDBCalcedAttribute = _pxContext.FieldAttributes.PXDBCalcedAttribute;
			_pxDBFieldAttribute = _pxContext.FieldAttributes.PXDBFieldAttribute;

			var attributesCalcedOnDbSide = new List<INamedTypeSymbol>(capacity: 2) { _pxDBScalarAttribute, _pxDBCalcedAttribute };
			AttributesCalcedOnDbSide = attributesCalcedOnDbSide.ToImmutableArray();

			UnboundDacFieldTypeAttributesWithFieldType = GetUnboundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext).ToImmutableDictionary();
			BoundDacFieldTypeAttributesWithFieldType = GetBoundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext).ToImmutableDictionary();

			AllDacFieldTypeAttributes = UnboundDacFieldTypeAttributesWithFieldType.Keys
										.Concat(BoundDacFieldTypeAttributesWithFieldType.Keys)
										.Concat(attributesCalcedOnDbSide)
										.ToImmutableHashSet();

			SortedDacFieldTypeAttributesWithMixedDbBoundness = GetDacFieldTypeAttributesWithMixedDbBoundness(_pxContext)
																.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
																.ToImmutableArray();

			WellKnownNonDataTypeAttributes = GetWellKnownNonDataTypeAttributes(_pxContext);
		}

		public bool IsWellKnownNonDataTypeAttribute(ITypeSymbol attribute)
		{
			var attributeTypeHierarchy = attribute.GetBaseTypesAndThis();
			return attributeTypeHierarchy.Any(type => WellKnownNonDataTypeAttributes.Contains(type));
		}

		public IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos(ITypeSymbol attributeSymbol) =>
			GetDacFieldTypeAttributeInfos(attributeSymbol, preparedFlattenedAttributes: null);

		internal IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos(ITypeSymbol attributeSymbol, 
																						  ImmutableHashSet<ITypeSymbol>? preparedFlattenedAttributes)
		{
			if (IsWellKnownNonDataTypeAttribute(attributeSymbol))
				return Array.Empty<DataTypeAttributeInfo>();

			var flattenedAttributes = preparedFlattenedAttributes ?? attributeSymbol.GetThisAndAllAggregatedAttributes(_pxContext, includeBaseTypes: true);
			return GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(attributeSymbol, flattenedAttributes);
		}

		internal IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(ITypeSymbol attributeSymbol,
																										ImmutableHashSet<ITypeSymbol> flattenedAttributes)
		{
			if (flattenedAttributes.Count == 0)
				return Array.Empty<DataTypeAttributeInfo>();

			var typeAttributeInfos = GetMixedDbBoundnessAttributeInfosInFlattenedSet(flattenedAttributes);
			var pxDbFieldAttributeDirectlyUsedInfo = GetPXDbFieldAttributeInfoIfItIsUsedDirectly(attributeSymbol);

			if (pxDbFieldAttributeDirectlyUsedInfo != null)
			{
				typeAttributeInfos ??= new List<DataTypeAttributeInfo>(capacity: 4);
				typeAttributeInfos.Add(pxDbFieldAttributeDirectlyUsedInfo);
			}

			var dacFieldTypeAttributesInHierarchy = AllDacFieldTypeAttributes.Intersect(flattenedAttributes);

			if (dacFieldTypeAttributesInHierarchy.Count == 0)
				return typeAttributeInfos as IReadOnlyCollection<DataTypeAttributeInfo> ?? Array.Empty<DataTypeAttributeInfo>();

			foreach (ITypeSymbol dacFieldTypeAttribute in dacFieldTypeAttributesInHierarchy)
			{
				DataTypeAttributeInfo? attributeInfo = GetDacFieldTypeAttributeInfo(dacFieldTypeAttribute);

				if (attributeInfo != null)
				{
					typeAttributeInfos ??= new List<DataTypeAttributeInfo>(capacity: 4);
					typeAttributeInfos.Add(attributeInfo);
				}
			}

			return typeAttributeInfos as IReadOnlyCollection<DataTypeAttributeInfo> ?? Array.Empty<DataTypeAttributeInfo>();
		}

		/// <summary>
		/// Gets mixed database boundness attribute infos in the flattened set with consideration of type hierarchy.<br/>
		/// If there are multiple mixed boundness attributes in the type hierarchy then only the most derived one should be included into results <br/>
		/// unless base mixed attribute is aggregated directly.
		/// </summary>
		/// <param name="attributeSymbol">The attribute symbol.</param>
		/// <param name="flattenedAttributes">The flattened attributes.</param>
		/// <returns>
		/// The mixed database boundness attribute infos from the flattened set of attributes.
		/// </returns>
		private List<MixedDbBoundnessAttributeInfo>? GetMixedDbBoundnessAttributeInfosInFlattenedSet(ITypeSymbol attributeSymbol, 
																									 ImmutableHashSet<ITypeSymbol> flattenedAttributes)
		{
			List<MixedDbBoundnessAttributeInfo>? mixedDbBoundnessAttributeInfos = null;
			HashSet<ITypeSymbol>? checkedMixedAttributes = null;

			foreach (MixedDbBoundnessAttributeInfo mixedBoundnessAttribute in SortedDacFieldTypeAttributesWithMixedDbBoundness)
			{
				if (!flattenedAttributes.Contains(mixedBoundnessAttribute.AttributeType))
					continue;

				bool isPartOfAnotherMixedAttributeHierarchy = checkedMixedAttributes?.Contains(mixedBoundnessAttribute.AttributeType) == true;

				if (isPartOfAnotherMixedAttributeHierarchy)
				{
					if (!attributeSymbol.EqualsOrAggregatesAttributeDirectly(mixedBoundnessAttribute.AttributeType, _pxContext))
						continue;
				}

				if (mixedDbBoundnessAttributeInfos == null)
				{
					mixedDbBoundnessAttributeInfos = new(capacity: 1) { mixedBoundnessAttribute };
					checkedMixedAttributes = mixedBoundnessAttribute.AcumaticaAttributesHierarchy.ToHashSet();
				}
				else
				{
					mixedDbBoundnessAttributeInfos.Add(mixedBoundnessAttribute);
					checkedMixedAttributes!.AddRange(mixedBoundnessAttribute.AcumaticaAttributesHierarchy);
				}				
			}

			return mixedDbBoundnessAttributeInfos;
		}

		private DataTypeAttributeInfo? GetPXDbFieldAttributeInfoIfItIsUsedDirectly(ITypeSymbol attributeSymbol) =>
			attributeSymbol.EqualsOrAggregatesAttributeDirectly(_pxDBFieldAttribute, _pxContext)
				? new DataTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, fieldType: null)
				: null;

		private DataTypeAttributeInfo? GetDacFieldTypeAttributeInfo(ITypeSymbol dacFieldTypeAttribute)
		{
			if (dacFieldTypeAttribute.Equals(_pxDBScalarAttribute))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.PXDBScalarAttribute, fieldType: null);
			else if (dacFieldTypeAttribute.Equals(_pxDBCalcedAttribute))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.PXDBCalcedAttribute, fieldType: null);
			else if (BoundDacFieldTypeAttributesWithFieldType.TryGetValue(dacFieldTypeAttribute, out var boundFieldType))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, boundFieldType);
			else if (UnboundDacFieldTypeAttributesWithFieldType.TryGetValue(dacFieldTypeAttribute, out var unboundFieldType))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.UnboundTypeAttribute, unboundFieldType);

			// TODO - some way of logging for Roslyn analyzers should be created with the support of out of process analysis
			return null;
		}

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


		/// <summary>
		/// Gets the DAC field type attributes with mixed DB boundness that can be put on both bound and unbound DAC field properties.
		/// </summary>
		/// <remarks>
		/// TODO: At this moment only the resolution of mixed DB boundness attribute is supported only for classic .Net inheritance. 
		/// The scenario where one mixed boundness attribute is aggregated on another mixed boundness attribute is not supported.
		/// </remarks>
		/// <param name="pxContext">The context.</param>
		/// <returns/>
		private static IEnumerable<MixedDbBoundnessAttributeInfo> GetDacFieldTypeAttributesWithMixedDbBoundness(PXContext pxContext) =>
			new List<MixedDbBoundnessAttributeInfo?>()
			{
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PeriodIDAttribute,					   pxContext.SystemTypes.String.Type, isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.AcctSubAttribute,					   pxContext.SystemTypes.Int32, isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.UnboundAccountAttribute,				   pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.UnboundCashAccountAttribute,			   pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.APTranRecognizedInventoryItemAttribute, pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PXEntityAttribute,					   null,						isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PXDBLocalizableStringAttribute,		   pxContext.SystemTypes.String.Type, isDbBoundByDefault: true),
			}
			.Where(attributeTypeWithIsDbFieldValue => attributeTypeWithIsDbFieldValue != null)!;

		private static ImmutableArray<INamedTypeSymbol> GetWellKnownNonDataTypeAttributes(PXContext pxContext)
		{
			var wellKnownNonDataTypeAttributes = ImmutableArray.CreateBuilder<INamedTypeSymbol>(initialCapacity: 6);

			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXUIFieldAttribute.Type!);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXDefaultAttribute);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXStringListAttribute.Type!);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXIntListAttribute.Type!);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXSelectorAttribute.Type!);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXForeignReferenceAttribute);

			return wellKnownNonDataTypeAttributes.ToImmutable();
		}
	}
}