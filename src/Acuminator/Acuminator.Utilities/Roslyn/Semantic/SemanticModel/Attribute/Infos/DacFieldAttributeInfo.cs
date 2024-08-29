#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about attributes of a DAC field.
	/// </summary>
	public class DacFieldAttributeInfo : AttributeInfoBase
	{
		public override AttributePlacement Placement => AttributePlacement.DacField;

		/// <summary>
		/// The flattened Acumatica attributes with the application set: the current attribute, its base attributes, 
		/// aggregated attributes in case of an aggregate attribute, 
		/// aggregates on aggregates and so on.
		/// </summary>
		public ImmutableHashSet<AttributeWithApplication> FlattenedAcumaticaAttributes { get; }

		/// <summary>
		/// The aggregated attribute metadata collection which is information from the flattened attributes set. 
		/// This information is mostly related to the attribute's relationship with the database.
		/// </summary>
		public ImmutableArray<DataTypeAttributeInfo> AggregatedAttributeMetadata { get; }

		public DbBoundnessType DbBoundness { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsDefaultAttribute { get; }

		public bool IsAutoNumberAttribute { get; }

		public bool IsAcumaticaAttribute { get; }

		protected DacFieldAttributeInfo(AttributeData attributeData, IEnumerable<AttributeWithApplication> flattenedAttributeApplications, 
										IEnumerable<DataTypeAttributeInfo> attributeInfos, DbBoundnessType dbBoundness, int declarationOrder, 
										bool isKey, bool isIdentity, bool isDefaultAttribute, bool isAutoNumberAttribute, 
										bool isAcumaticaAttribute) :
								   base(attributeData, declarationOrder)
		{
			FlattenedAcumaticaAttributes = (flattenedAttributeApplications as ImmutableHashSet<AttributeWithApplication>) ?? flattenedAttributeApplications.ToImmutableHashSet();
			AggregatedAttributeMetadata  = attributeInfos.ToImmutableArray();
			DbBoundness                  = dbBoundness;
			IsKey                        = isKey;
			IsIdentity                   = isIdentity;
			IsDefaultAttribute           = isDefaultAttribute;
			IsAutoNumberAttribute        = isAutoNumberAttribute;
			IsAcumaticaAttribute         = isAcumaticaAttribute;
		}

		public static DacFieldAttributeInfo Create(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder) =>
			CreateUnsafe(attribute.CheckIfNull(), dbBoundnessCalculator.CheckIfNull(), declarationOrder);

		public static DacFieldAttributeInfo CreateUnsafe(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder)
		{
			var flattenedAttributeApplications = attribute.GetThisAndAllAggregatedAttributesWithApplications(dbBoundnessCalculator.Context, includeBaseTypes: true);
			var flattenedAttributeTypes = flattenedAttributeApplications.Count > 0
				? flattenedAttributeApplications.Select(attributeWithApplication => attributeWithApplication.Type).ToImmutableHashSet()
				: ImmutableHashSet<ITypeSymbol>.Empty;

			var aggregatedMetadata	    = dbBoundnessCalculator.AttributesMetadataProvider
															   .GetDacFieldTypeAttributeInfos(attribute.AttributeClass, flattenedAttributeTypes);
			DbBoundnessType dbBoundness = dbBoundnessCalculator.GetAttributeApplicationDbBoundnessType(attribute, flattenedAttributeApplications, 
																										flattenedAttributeTypes, aggregatedMetadata);

			bool isPXDefaultAttribute      = IsPXDefaultAttribute(flattenedAttributeTypes, dbBoundnessCalculator.Context);
			bool isIdentityAttribute       = IsDerivedFromIdentityTypes(flattenedAttributeTypes, dbBoundnessCalculator.Context);
			bool isAutoNumberAttribute     = CheckForAutoNumberAttribute(flattenedAttributeTypes, dbBoundnessCalculator.Context);
			bool isAttributeWithPrimaryKey = attribute.NamedArguments.Any(arg => arg.Key.Contains(PropertyNames.Attributes.IsKey) &&
																				 arg.Value.Value is bool isKeyValue && isKeyValue == true);
			bool isAcumaticaAttribute	   = IsAcumaticaPlatformAttribute(flattenedAttributeTypes, attribute.AttributeClass, dbBoundnessCalculator.Context);

			return new DacFieldAttributeInfo(attribute, flattenedAttributeApplications, aggregatedMetadata, dbBoundness, 
											 declarationOrder, isAttributeWithPrimaryKey, isIdentityAttribute, isPXDefaultAttribute, isAutoNumberAttribute,
											 isAcumaticaAttribute);
		}

		private static bool IsDerivedFromIdentityTypes(ImmutableHashSet<ITypeSymbol> flattenedAttributes, PXContext pxContext) =>
			flattenedAttributes.Contains(pxContext.FieldAttributes.PXDBIdentityAttribute) ||
			flattenedAttributes.Contains(pxContext.FieldAttributes.PXDBLongIdentityAttribute);

		private static bool IsPXDefaultAttribute(ImmutableHashSet<ITypeSymbol> flattenedAttributes, PXContext pxContext) =>
			flattenedAttributes.Contains(pxContext.AttributeTypes.PXDefaultAttribute) && 
			!flattenedAttributes.Contains(pxContext.AttributeTypes.PXUnboundDefaultAttribute);
		
		private static bool CheckForAutoNumberAttribute(ImmutableHashSet<ITypeSymbol> flattenedAttributes, PXContext pxContext)
		{
			var autoNumberAttribute = pxContext.AttributeTypes.AutoNumberAttribute.Type;

			if (autoNumberAttribute == null)
				return false;

			return flattenedAttributes.Contains(autoNumberAttribute);
		}

		private static bool IsAcumaticaPlatformAttribute(ImmutableHashSet<ITypeSymbol> flattenedAttributes, INamedTypeSymbol attributeType, 
														 PXContext pxContext) =>
			flattenedAttributes.Count > 0 || pxContext.AttributeTypes.PXEventSubscriberAttribute.Equals(attributeType);
	}
}