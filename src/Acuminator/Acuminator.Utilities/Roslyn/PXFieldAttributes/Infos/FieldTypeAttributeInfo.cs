#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attribute.
	/// </summary>
	public class FieldTypeAttributeInfo : IEquatable<FieldTypeAttributeInfo>
	{
		public ITypeSymbol? FieldType { get; }

		public FieldTypeAttributeKind Kind { get; }

		public bool IsSpecial =>
			Kind == FieldTypeAttributeKind.PXDBCalcedAttribute || Kind == FieldTypeAttributeKind.PXDBScalarAttribute;

		public bool IsFieldAttribute =>
			Kind == FieldTypeAttributeKind.BoundTypeAttribute || 
			Kind == FieldTypeAttributeKind.UnboundTypeAttribute || 
			Kind == FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute;

		public FieldTypeAttributeInfo(FieldTypeAttributeKind attributeKind, ITypeSymbol? fieldType)
		{
			FieldType = fieldType;
			Kind = attributeKind;
		}

		public override bool Equals(object obj) => Equals(obj as FieldTypeAttributeInfo);

		public virtual bool Equals(FieldTypeAttributeInfo? other)
		{
			if (ReferenceEquals(this, other))
				return true;

			return Kind == other?.Kind && object.Equals(FieldType, other.FieldType) &&
				   GetType() == other.GetType();
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Kind.GetHashCode();
				hash = 23 * hash + (FieldType?.GetHashCode() ?? 0);
			}

			return hash;
		}
	}
}
