#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attributes with mixed DB boundness.
	/// </summary>
	public class MixedDbBoundnessAttributeInfo : DataTypeAttributeInfo, IEquatable<MixedDbBoundnessAttributeInfo>
	{
		/// <summary>
		/// The type of the attribute.
		/// </summary>
		public INamedTypeSymbol AttributeType { get; }

		public IReadOnlyCollection<ITypeSymbol> AcumaticaAttributesHierarchy { get; }

		/// <summary>
		/// The value indicating whether the field type attribute is database bound by default. If null then the attribute does not have a default DB boundness.
		/// </summary>
		public bool? IsDbBoundByDefault { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attributeType">The type of the attribute. Only Acumatica attributes should be passed</param>
		/// <param name="fieldType">Type of the field.</param>
		/// <param name="isDbBoundByDefault">The value indicating whether the field type attribute is database bound by default. If null then the attribute does not have a default DB boundness.</param>
		internal MixedDbBoundnessAttributeInfo(INamedTypeSymbol attributeType, ITypeSymbol? fieldType, bool? isDbBoundByDefault) : 
										  base(FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute, fieldType)
		{
			AcumaticaAttributesHierarchy = attributeType.GetBaseTypesAndThis()
														.TakeWhile(baseAttributeType => baseAttributeType.ToString() != TypeFullNames.PXEventSubscriberAttribute)
														.ToImmutableArray();
			AttributeType	   = attributeType;
			IsDbBoundByDefault = isDbBoundByDefault;
		}

		internal static MixedDbBoundnessAttributeInfo? Create(INamedTypeSymbol? attributeType, ITypeSymbol? fieldType, bool? isDbBoundByDefault) =>
			attributeType != null
				? new MixedDbBoundnessAttributeInfo(attributeType, fieldType, isDbBoundByDefault)
				: null;

		public override bool Equals(object obj) => Equals(obj as MixedDbBoundnessAttributeInfo);

		public override bool Equals(DataTypeAttributeInfo? other) => Equals(other as MixedDbBoundnessAttributeInfo);

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

		public override string ToString() => IsDbBoundByDefault.HasValue
			? $"{base.ToString()} {AttributeType} {IsDbBoundByDefault.Value}"
			: $"{base.ToString()} {AttributeType}";
	}
}
