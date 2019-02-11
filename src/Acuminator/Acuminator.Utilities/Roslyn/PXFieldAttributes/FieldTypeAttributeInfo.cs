using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field type attributes.
	/// </summary>
	public readonly struct FieldTypeAttributeInfo
	{
		public ITypeSymbol FieldType { get; }

		public FieldTypeAttributeKind Kind { get; }

		public bool IsSpecial =>
			Kind != FieldTypeAttributeKind.BoundTypeAttribute &&
			Kind != FieldTypeAttributeKind.UnboundTypeAttribute;

		public bool IsFieldAttribute =>
			Kind == FieldTypeAttributeKind.BoundTypeAttribute ||
			Kind == FieldTypeAttributeKind.UnboundTypeAttribute;

		public FieldTypeAttributeInfo(FieldTypeAttributeKind attributeKind, ITypeSymbol fieldType)
		{
			FieldType = fieldType;
			Kind = attributeKind;
		}
	}
}
