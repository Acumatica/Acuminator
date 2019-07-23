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

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";

		public AttributeInfo(AttributeData attributeData, BoundType boundType, int declarationOrder, bool isKey, bool isIdentity)
		{
			attributeData.ThrowOnNull(nameof(attributeData));
			AttributeData = attributeData;
			BoundType = boundType;
			DeclarationOrder = declarationOrder;
			IsKey = isKey;
			IsIdentity = isIdentity;
		}

		public static AttributeInfo Create(AttributeData attribute, AttributeInformation attributeInformation, int declarationOrder)
		{
			attribute.ThrowOnNull(nameof(attribute));
			attributeInformation.ThrowOnNull(nameof(attributeInformation));

			BoundType boundType = attributeInformation.GetBoundAttributeType(attribute);
			bool isIdentityAttribute = IsDerivedFromIdentityTypes(attribute, attributeInformation);
			bool isAttributeWithPrimaryKey = attribute.NamedArguments.Any(arg => arg.Key.Contains(DelegateNames.IsKey) &&
																				 arg.Value.Value is bool isKeyValue && isKeyValue == true);

			return new AttributeInfo(attribute, boundType, declarationOrder, isAttributeWithPrimaryKey, isIdentityAttribute);
		}

		private static bool IsDerivedFromIdentityTypes(AttributeData attribute, AttributeInformation attributeInformation) =>
			attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, attributeInformation.Context.FieldAttributes.PXDBIdentityAttribute) ||
			attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, attributeInformation.Context.FieldAttributes.PXDBLongIdentityAttribute);
	}
}
