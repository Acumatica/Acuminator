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

		public BoundType BoundType { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsDefaultAttribute { get; }

		public bool IsAutoNumberAttribute { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";

		protected AttributeInfo(AttributeData attributeData, BoundType boundType, int declarationOrder, bool isKey, bool isIdentity, 
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

		public static AttributeInfo Create(AttributeData attribute, AttributeInformation attributeInformation, int declarationOrder)
		{
			attribute.ThrowOnNull(nameof(attribute));
			attributeInformation.ThrowOnNull(nameof(attributeInformation));

			BoundType boundType = attributeInformation.GetBoundAttributeType(attribute);
			bool isPXDefaultAttribute = IsPXDefaultAttribute(attribute, attributeInformation);
			bool isIdentityAttribute = IsDerivedFromIdentityTypes(attribute, attributeInformation);
			bool isAutoNumberAttribute = CheckForAutoNumberAttribute(attribute, attributeInformation);
			bool isAttributeWithPrimaryKey = attribute.NamedArguments.Any(arg => arg.Key.Contains(DelegateNames.IsKey) &&
																				 arg.Value.Value is bool isKeyValue && isKeyValue == true);

			return new AttributeInfo(attribute, boundType, declarationOrder, isAttributeWithPrimaryKey, isIdentityAttribute,
									 isPXDefaultAttribute, isAutoNumberAttribute);
		}

		private static bool IsDerivedFromIdentityTypes(AttributeData attribute, AttributeInformation attributeInformation) =>
			attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, attributeInformation.Context.FieldAttributes.PXDBIdentityAttribute) ||
			attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, attributeInformation.Context.FieldAttributes.PXDBLongIdentityAttribute);

		private static bool IsPXDefaultAttribute(AttributeData attribute, AttributeInformation attributeInformation)
		{
			var pxDefaultAttribute = attributeInformation.Context.AttributeTypes.PXDefaultAttribute;
			var pxUnboundDefaultAttribute = attributeInformation.Context.AttributeTypes.PXUnboundDefaultAttribute;

			return attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxDefaultAttribute) &&
				   !attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxUnboundDefaultAttribute);
		}

		private static bool CheckForAutoNumberAttribute(AttributeData attribute, AttributeInformation attributeInformation)
		{
			var autoNumberAttribute = attributeInformation.Context.AttributeTypes.AutoNumberAttribute.Type;

			if (autoNumberAttribute == null)
				return false;

			return attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, autoNumberAttribute);
		}
	}
}
