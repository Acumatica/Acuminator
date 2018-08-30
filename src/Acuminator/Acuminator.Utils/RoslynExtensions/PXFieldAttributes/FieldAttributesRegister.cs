using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
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
		private readonly AttributeInformation attributeInformation;

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol> CorrespondingSimpleTypes { get; }

		public ImmutableHashSet<ITypeSymbol> UnboundFieldAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> BoundFieldAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> AllFieldAttributes { get; }

		public FieldAttributesRegister(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			context = pxContext;
			attributeInformation = new AttributeInformation(context);
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

			
			List<ITypeSymbol> attributeTypeHierarchy = attributeInformation.AttributesListDerivedFromClass(attributeSymbol, true)
																		   .ToList();

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
			var attributeBaseTypesEnum = attributeTypeHierarchy ?? attributeInformation.AttributesListDerivedFromClass( attributeSymbol,true)
																						.ToList();

			ITypeSymbol fieldAttribute = attributeBaseTypesEnum.TakeWhile(a => !a.Equals(context.FieldAttributes.PXDBScalarAttribute))
															   .FirstOrDefault(a => AllFieldAttributes.Contains(a));

			if (fieldAttribute == null)
				return null;

			bool isBoundField = BoundFieldAttributes.Contains(fieldAttribute);
			return CorrespondingSimpleTypes.TryGetValue(fieldAttribute, out var fieldType)
				? new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType)
				: new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType: null);
		}

		private static HashSet<ITypeSymbol> GetUnboundFieldAttributes(PXContext pxContext) =>
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

		private static HashSet<ITypeSymbol> GetBoundFieldAttributes(PXContext pxContext) =>
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
				pxContext.FieldAttributes.PXDBCalcedAttribute
				pxContext.FieldAttributes.PXDBAttributeAttribute,
				pxContext.FieldAttributes.PXDBDataLengthAttribute,
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
