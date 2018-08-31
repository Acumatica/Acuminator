using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public readonly struct FieldAttributeInfo
	{
		public bool IsFieldAttribute { get; }

		public bool IsBoundField { get; }

		public ITypeSymbol FieldType { get; }

		public FieldAttributeInfo(bool isFieldAttribute, bool isBoundField, ITypeSymbol fieldType)
		{
			IsFieldAttribute = isFieldAttribute;
			IsBoundField = isBoundField;
			FieldType = fieldType;
		}		
	}
}
