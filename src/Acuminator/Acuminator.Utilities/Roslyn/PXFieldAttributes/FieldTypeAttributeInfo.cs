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

		public bool IsDBCalcAttribute { get; }

		public FieldTypeAttributeInfo(bool isFieldTypeAttribute, bool isDBCalcAttribute, ITypeSymbol fieldType)
		{
			IsFieldAttribute = isFieldTypeAttribute;
			FieldType = fieldType;
			IsDBCalcAttribute = isDBCalcAttribute;
		}		
	}
}
