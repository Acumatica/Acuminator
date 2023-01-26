#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	///  A  class for attributes
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AttributeInfo
	{
		/// <summary>
		/// Information describing the attribute application.
		/// </summary>
		public AttributeData AttributeData { get; }

		public INamedTypeSymbol AttributeType => AttributeData.AttributeClass;

		/// <summary>
		/// The flattened Acumatica attributes set - this attribute, its base attributes, aggregated attributes in case of an aggregate attribute, 
		/// aggregates on aggregates and so on.
		/// </summary>
		public ImmutableHashSet<ITypeSymbol> FlattenedAcumaticaAttributes { get; }

		/// <summary>
		/// The aggregated attribute metadata collection - information from the flattened attributes set. 
		/// It is mostly related to the attribute's relationship with database.
		/// </summary>
		public ImmutableArray<DataTypeAttributeInfo> AggregatedAttributeMetadata { get; }

		public virtual string Name => AttributeType.Name;

		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		public DbBoundnessType BoundnessType { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsDefaultAttribute { get; }

		public bool IsAutoNumberAttribute { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";

		protected AttributeInfo(AttributeData attributeData, IEnumerable<ITypeSymbol> flattenedAttributes, IEnumerable<DataTypeAttributeInfo> attributeInfos,
								DbBoundnessType boundnessType, int declarationOrder, bool isKey, bool isIdentity, bool isDefaultAttribute, bool isAutoNumberAttribute)
		{
			AttributeData                = attributeData.CheckIfNull(nameof(attributeData));
			FlattenedAcumaticaAttributes = (flattenedAttributes as ImmutableHashSet<ITypeSymbol>) ?? flattenedAttributes.ToImmutableHashSet();
			AggregatedAttributeMetadata  = attributeInfos.ToImmutableArray();
			BoundnessType                = boundnessType;
			DeclarationOrder             = declarationOrder;
			IsKey                        = isKey;
			IsIdentity                   = isIdentity;
			IsDefaultAttribute           = isDefaultAttribute;
			IsAutoNumberAttribute        = isAutoNumberAttribute;
		}

		public static AttributeInfo Create(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder)
		{
			attribute.ThrowOnNull(nameof(attribute));
			dbBoundnessCalculator.ThrowOnNull(nameof(dbBoundnessCalculator));

			var flattenedAttributes = attribute.AttributeClass.GetThisAndAllAggregatedAttributes(dbBoundnessCalculator.Context, includeBaseTypes: true);
			var aggregatedMetadata = dbBoundnessCalculator.AttributesMetadataProvider.GetDacFieldTypeAttributeInfos(attribute.AttributeClass, flattenedAttributes);

			DbBoundnessType boundType      = dbBoundnessCalculator.GetAttributeApplicationDbBoundnessType(attribute, flattenedAttributes, aggregatedMetadata);
			bool isPXDefaultAttribute      = IsPXDefaultAttribute(flattenedAttributes, dbBoundnessCalculator.Context);
			bool isIdentityAttribute       = IsDerivedFromIdentityTypes(flattenedAttributes, dbBoundnessCalculator.Context);
			bool isAutoNumberAttribute     = CheckForAutoNumberAttribute(flattenedAttributes, dbBoundnessCalculator.Context);
			bool isAttributeWithPrimaryKey = attribute.NamedArguments.Any(arg => arg.Key.Contains(PropertyNames.Attributes.IsKey) &&
																				 arg.Value.Value is bool isKeyValue && isKeyValue == true);

			return new AttributeInfo(attribute, flattenedAttributes, aggregatedMetadata, boundType, declarationOrder, isAttributeWithPrimaryKey,
									 isIdentityAttribute, isPXDefaultAttribute, isAutoNumberAttribute);
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
	}
}