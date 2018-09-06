using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attributes.
	/// </summary>
	public readonly struct FieldTypeAttributeInfo
	{
		public bool IsFieldAttribute { get; }

		public ITypeSymbol FieldType { get; }

		public FieldTypeAttributeKind Kind { get; }

		public bool IsSpecial => Kind != FieldTypeAttributeKind.TypeAttribute;

		public FieldTypeAttributeInfo(bool isFieldTypeAttribute, FieldTypeAttributeKind attributeKind, ITypeSymbol fieldType)
		{
			IsFieldAttribute = isFieldTypeAttribute;
			FieldType = fieldType;
			Kind = attributeKind;
		}
	}
}
