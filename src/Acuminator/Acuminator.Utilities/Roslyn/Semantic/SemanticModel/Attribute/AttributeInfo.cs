using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	///  A  class for attributes
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AttributeInfo
	{
		/// <summary>
		/// Information describing the attribute.
		/// </summary>
		public AttributeData AttributeData { get; }

		public INamedTypeSymbol AttributeType => AttributeData.AttributeClass;

		public virtual string Name => AttributeType.Name;

		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		public DbBoundnessType BoundType { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsDefaultAttribute { get; }

		public bool IsAutoNumberAttribute { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";

		protected AttributeInfo(AttributeData attributeData, DbBoundnessType boundType, int declarationOrder, bool isKey, bool isIdentity, 
								bool isDefaultAttribute, bool isAutoNumberAttribute)
		{
			attributeData.ThrowOnNull(nameof(attributeData));
			AttributeData = attributeData;
			BoundType = boundType;
			DeclarationOrder = declarationOrder;
			IsKey = isKey;
			IsIdentity = isIdentity;
			IsDefaultAttribute = isDefaultAttribute;
			IsAutoNumberAttribute = isAutoNumberAttribute;
		}

		public static AttributeInfo Create(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder)
		{
			attribute.ThrowOnNull(nameof(attribute));
			dbBoundnessCalculator.ThrowOnNull(nameof(dbBoundnessCalculator));

			DbBoundnessType boundType = dbBoundnessCalculator.GetAttributeApplicationDbBoundnessType(attribute);
			bool isPXDefaultAttribute = IsPXDefaultAttribute(attribute, dbBoundnessCalculator.Context);
			bool isIdentityAttribute = IsDerivedFromIdentityTypes(attribute, dbBoundnessCalculator.Context);
			bool isAutoNumberAttribute = CheckForAutoNumberAttribute(attribute, dbBoundnessCalculator.Context);
			bool isAttributeWithPrimaryKey = attribute.NamedArguments.Any(arg => arg.Key.Contains(PropertyNames.Attributes.IsKey) &&
																				 arg.Value.Value is bool isKeyValue && isKeyValue == true);

			return new AttributeInfo(attribute, boundType, declarationOrder, isAttributeWithPrimaryKey, isIdentityAttribute,
									 isPXDefaultAttribute, isAutoNumberAttribute);
		}

		private static bool IsDerivedFromIdentityTypes(AttributeData attribute, PXContext pxContext) =>
			attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(pxContext.FieldAttributes.PXDBIdentityAttribute, pxContext) ||
			attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(pxContext.FieldAttributes.PXDBLongIdentityAttribute, pxContext);

		private static bool IsPXDefaultAttribute(AttributeData attribute, PXContext pxContext)
		{
			var pxDefaultAttribute = pxContext.AttributeTypes.PXDefaultAttribute;
			var pxUnboundDefaultAttribute = pxContext.AttributeTypes.PXUnboundDefaultAttribute;

			return attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(pxDefaultAttribute, pxContext) &&
				   !attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(pxUnboundDefaultAttribute, pxContext);
		}

		private static bool CheckForAutoNumberAttribute(AttributeData attribute, PXContext pxContext)
		{
			var autoNumberAttribute = pxContext.AttributeTypes.AutoNumberAttribute.Type;

			if (autoNumberAttribute == null)
				return false;

			return attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(autoNumberAttribute, pxContext);
		}
	}
}