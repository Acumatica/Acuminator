using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class FieldTypeAttributesOnDacPropertyTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldTypeAttributes.cs")]
		public virtual void Property_With_Multiple_FieldTypeAttributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 24, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 25, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithMultipleSpecialTypeAttributes.cs")]
		public virtual void Property_With_MultipleSpecialAttributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty.CreateFor(line: 16, column: 4),
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty.CreateFor(line: 17, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldTypeAttributes_Expected.cs")]
		public virtual Task MultipleFieldTypeAttributes_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithMultipleSpecialTypeAttributes_Expected.cs")]
		public virtual Task MultipleSpecialAttributes_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch_Expected.cs")]
		public virtual Task DacPropertyType_NotMatching_AttributeType_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);	

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch.cs")]
		public virtual void DacPropertyType_NotMatching_AttributeType(string source) =>
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
		[EmbeddedFileData("DacWithInvalidAggregatorAttributes.cs")]
		public virtual void DacProperty_With_Invalid_AggregatorAttributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators.CreateFor(line: 41, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators.CreateFor(line: 55, column: 4));

		[Theory]
		[EmbeddedFileData("DacFieldTypeMismatchPXDBScalarAttr.cs")]
		public virtual void Dac_Property_With_PXDBScalar_Attribute(string source) =>
			VerifyCSharpDiagnostic(source);	
	}
}
