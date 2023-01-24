#nullable enable

using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attributes with mixed DB boundness.
	/// </summary>
	public class MixedDbBoundnessAttributeInfo : FieldTypeAttributeInfo, IEquatable<MixedDbBoundnessAttributeInfo>
	{
		/// <summary>
		/// The type of the attribute.
		/// </summary>
		public INamedTypeSymbol AttributeType { get; }

		/// <summary>
		/// The value indicating whether the field type attribute is database bound by default. If null then the attribute does not have a default DB boundness.
		/// </summary>
		public bool? IsDbBoundByDefault { get; }

		public MixedDbBoundnessAttributeInfo(INamedTypeSymbol attributeType, ITypeSymbol? fieldType, bool? isDbBoundByDefault) : 
										base(FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute, fieldType)
		{
			AttributeType = attributeType.CheckIfNull(nameof(attributeType));
			IsDbBoundByDefault = isDbBoundByDefault;
		}

		public static MixedDbBoundnessAttributeInfo? Create(INamedTypeSymbol? attributeType, ITypeSymbol? fieldType, bool? isDbBoundByDefault) =>
			attributeType != null
				? new MixedDbBoundnessAttributeInfo(attributeType, fieldType, isDbBoundByDefault)
				: null;

		public override bool Equals(object obj) => Equals(obj as MixedDbBoundnessAttributeInfo);

		public override bool Equals(FieldTypeAttributeInfo? other) => Equals(other as MixedDbBoundnessAttributeInfo);

		public bool Equals(MixedDbBoundnessAttributeInfo? other) =>
			base.Equals(other) && IsDbBoundByDefault == other.IsDbBoundByDefault && AttributeType.Equals(other.AttributeType);

		public override int GetHashCode()
		{
			int hash = base.GetHashCode();

			unchecked
			{
				hash = 23 * hash + IsDbBoundByDefault.GetValueOrDefault().GetHashCode();
				hash = 23 * hash + AttributeType.GetHashCode();
			}

			return hash;
		}
	}
}
