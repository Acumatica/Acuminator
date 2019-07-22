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
	/// Information about the Acumatica field type attributes.
	/// </summary>
	public class FieldTypeAttributesRegister
	{
		public PXContext Context { get; }

		public AttributeInformation AttributeInformation { get; }

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol> CorrespondingSimpleUnboundTypes { get; }

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol> CorrespondingSimpleBoundTypes { get; }

		public ImmutableHashSet<ITypeSymbol> UnboundTypeAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> BoundTypeAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> SpecialAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> AllTypeAttributes { get; }

		public FieldTypeAttributesRegister(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			Context = pxContext;
			AttributeInformation = new AttributeInformation(Context);
			var unboundFieldAttributes = GetCorrespondingSimpleUnboundTypes(Context).Keys;
			UnboundTypeAttributes = unboundFieldAttributes.ToImmutableHashSet();

			var boundFieldAttributes = GetCorrespondingSimpleBoundTypes(Context).Keys;
			BoundTypeAttributes = boundFieldAttributes.ToImmutableHashSet();

			var specialAttributes = GetSpecialAttributes(Context);
			SpecialAttributes = specialAttributes.ToImmutableHashSet();
			AllTypeAttributes = unboundFieldAttributes.Concat(boundFieldAttributes)
													  .Concat(specialAttributes)
													  .ToImmutableHashSet();

			CorrespondingSimpleUnboundTypes = GetCorrespondingSimpleUnboundTypes(Context).ToImmutableDictionary();
			CorrespondingSimpleBoundTypes = GetCorrespondingSimpleBoundTypes(Context).ToImmutableDictionary();
		}

		public IEnumerable<FieldTypeAttributeInfo> GetFieldTypeAttributeInfos(ITypeSymbol attributeSymbol)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));

			var expandedAttributes = AttributeInformation.GetAcumaticaAttributesFullList(attributeSymbol);

			if (expandedAttributes.IsNullOrEmpty())
				return Enumerable.Empty<FieldTypeAttributeInfo>();

			List<FieldTypeAttributeInfo> typeAttributeInfos = new List<FieldTypeAttributeInfo>(capacity: 2);

			foreach (ITypeSymbol attribute in expandedAttributes)
			{
				FieldTypeAttributeInfo? attributeInfo = GetTypeAttributeInfo(attribute);

				if (attributeInfo != null)
				{
					typeAttributeInfos.Add(attributeInfo.Value);
				}
			}

			return typeAttributeInfos;
		}

		private FieldTypeAttributeInfo? GetTypeAttributeInfo(ITypeSymbol typeAttribute)
		{
			var firstTypeAttribute = typeAttribute.GetBaseTypesAndThis()
												  .FirstOrDefault(type => AllTypeAttributes.Contains(type));
			if (firstTypeAttribute == null)
				return null;

			if (firstTypeAttribute.Equals(Context.FieldAttributes.PXDBScalarAttribute))
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.PXDBScalarAttribute, fieldType: null);
			else if (firstTypeAttribute.Equals(Context.FieldAttributes.PXDBCalcedAttribute))
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.PXDBCalcedAttribute, fieldType: null);

			if (CorrespondingSimpleBoundTypes.TryGetValue(firstTypeAttribute, out var boundFieldType))
			{
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, boundFieldType);
			}

			if (CorrespondingSimpleUnboundTypes.TryGetValue(firstTypeAttribute, out var unboundFieldType))
			{
				return new FieldTypeAttributeInfo(FieldTypeAttributeKind.UnboundTypeAttribute, unboundFieldType);
			}

			throw new InvalidOperationException($"Cannot get type attribute info for {typeAttribute}");
		}

		private static HashSet<ITypeSymbol> GetSpecialAttributes(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXDBScalarAttribute,
				pxContext.FieldAttributes.PXDBCalcedAttribute
			};

		private static Dictionary<ITypeSymbol, ITypeSymbol> GetCorrespondingSimpleUnboundTypes(PXContext pxContext)
		{
			return new Dictionary<ITypeSymbol, ITypeSymbol>
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
			};
		}

		private static Dictionary<ITypeSymbol, ITypeSymbol> GetCorrespondingSimpleBoundTypes(PXContext pxContext)
		{
			var types = new Dictionary<ITypeSymbol, ITypeSymbol>
			{
				{ pxContext.FieldAttributes.PXDBFieldAttribute, null },
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

			var packagedIntegerAttribute = pxContext.FieldAttributes.PXDBPackedIntegerArrayAttribute;

			if (packagedIntegerAttribute != null)
			{
				types.Add(packagedIntegerAttribute, pxContext.SystemTypes.UInt16Array);
			}

			return types;
		}
	}
}