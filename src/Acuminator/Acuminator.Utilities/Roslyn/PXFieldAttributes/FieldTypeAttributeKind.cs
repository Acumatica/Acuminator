namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Acumatica field type attribute kinds.
	/// </summary>
	public enum FieldTypeAttributeKind
	{
		/// <summary>
		/// DB bound type attribute
		/// </summary>
		BoundTypeAttribute,

		/// <summary>
		/// DB unbound type attribute
		/// </summary>
		UnboundTypeAttribute,

		/// <summary>
		/// PXDBScalarAttribute attribute.
		/// </summary>
		PXDBScalarAttribute,

		/// <summary>
		/// PXDBCalcedAttribute attribute.
		/// </summary>
		PXDBCalcedAttribute
	}
}
