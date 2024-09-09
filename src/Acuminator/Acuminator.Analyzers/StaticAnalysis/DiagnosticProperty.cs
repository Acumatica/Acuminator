
namespace Acuminator.Analyzers.StaticAnalysis
{
	/// <summary>
	/// A class with string constatns representing diagnostic property names. They are used to pass custom data strings from diagnostic to code fix.
	/// </summary>
	internal static class DiagnosticProperty
	{
		
		/// <summary>
		/// The property responsible for code fix registration.
		/// </summary>
		public const string RegisterCodeFix = nameof(RegisterCodeFix);

		/// <summary>
		/// The property used to pass the name of the DAC with the diagnostic.
		/// </summary>
		public const string DacName = nameof(DacName);

		/// <summary>
		/// The property used to pass the name of the DAC field with the diagnostic.
		/// </summary>
		public const string DacFieldName = nameof(DacFieldName);

		/// <summary>
		/// The property used to pass the DAC metadata with the diagnostic.
		/// </summary>
		public const string DacMetadataName = nameof(DacMetadataName);

		/// <summary>
		/// The property used to pass the field information was gotten from attributes.
		/// </summary>
		public const string IsBoundField = nameof(IsBoundField);

		/// <summary>
		/// The property used to pass the information about a property type, usually, a DAC property.
		/// </summary>
		public const string PropertyType = nameof(PropertyType);

		/// <summary>
		/// The property used to pass the information about a DAC BQL field type like BqlString.
		/// </summary>
		public const string BqlFieldType = nameof(BqlFieldType);

		/// <summary>
		/// The property used to pass the information that the diagnostic is reported on the property. 
		/// </summary>
		public const string IsProperty = nameof(IsProperty);
	}
}
