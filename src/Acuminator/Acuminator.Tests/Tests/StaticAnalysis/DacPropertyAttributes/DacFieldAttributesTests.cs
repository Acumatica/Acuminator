using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class DacFieldAttributesTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldAttributes.cs")]
		public virtual void Test_Dac_With_Multiple_Field_Attributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_DacPropertyMultipleFieldAttributes.CreateFor(line: 24, column: 4),
				Descriptors.PX1023_DacPropertyMultipleFieldAttributes.CreateFor(line: 25, column: 4));

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch.cs")]
		public virtual void Test_Dac_With_Property_Type_Not_Matching_Field_Attribute_Type(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 24, column: 4), 
					extraLocation: (line: 26, column: 10)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 26, column: 10), 
					extraLocation: (line: 24, column: 4)),

				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 34, column: 4),
					extraLocation: (line: 35, column: 18)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 35, column: 18),
					extraLocation: (line: 34, column: 4)),

				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 53, column: 4), 
					extraLocation: (line: 54, column: 18)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((line: 54, column: 18),
					extraLocation: (line: 53, column: 4)));

		[Theory]
		[EmbeddedFileData("DacFieldTypeMismatchPXDBScalarAttr.cs")]
		public virtual void Test_Dac_Property_With_PXDBScalar_Attribute(string source) =>
			VerifyCSharpDiagnostic(source);	
	}
}
