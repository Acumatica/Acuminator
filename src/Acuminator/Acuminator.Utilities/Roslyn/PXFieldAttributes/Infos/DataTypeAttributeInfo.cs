#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica data type attribute.
	/// </summary>
	public class DataTypeAttributeInfo : IEquatable<DataTypeAttributeInfo>
	{
		public ITypeSymbol? DataType { get; }

		public FieldTypeAttributeKind Kind { get; }

		public bool IsCalculatedOnDbSide =>
			Kind == FieldTypeAttributeKind.PXDBCalcedAttribute || Kind == FieldTypeAttributeKind.PXDBScalarAttribute;

		public bool IsFieldAttribute =>
			Kind == FieldTypeAttributeKind.BoundTypeAttribute || 
			Kind == FieldTypeAttributeKind.UnboundTypeAttribute || 
			Kind == FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute;

		public DataTypeAttributeInfo(FieldTypeAttributeKind attributeKind, ITypeSymbol? fieldType)
		{
			DataType = fieldType;
			Kind = attributeKind;
		}

		public override bool Equals(object obj) => Equals(obj as DataTypeAttributeInfo);

		public virtual bool Equals(DataTypeAttributeInfo? other)
		{
			if (ReferenceEquals(this, other))
				return true;

			return Kind == other?.Kind && SymbolEqualityComparer.Default.Equals(DataType, other.DataType) &&
				   GetType() == other.GetType();
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Kind.GetHashCode();
				hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(DataType);
			}

			return hash;
		}

		public override string ToString() => Kind.ToString();
	}
}
