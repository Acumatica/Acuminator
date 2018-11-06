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
		public virtual void PropertyWithMultipleFieldTypeAttributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 24, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 25, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithMultipleSpecialTypeAttributes.cs")]
		public virtual void PropertyWithMultipleSpecialAttributes(string source) =>
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
		public virtual Task DacPropertyTypeNotMatchingAttributeType_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);	

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch.cs")]
		public virtual void DacPropertyTypeNotMatchingAttributeType(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 24, Column: 4), 
					extraLocation: (Line: 26, Column: 10)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 26, Column: 10), 
					extraLocation: (Line: 24, Column: 4)),

				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 34, Column: 4),
					extraLocation: (Line: 35, Column: 18)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 35, Column: 18),
					extraLocation: (Line: 34, Column: 4)),

				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 53, Column: 4), 
					extraLocation: (Line: 54, Column: 18)),
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.CreateFor((Line: 54, Column: 18),
					extraLocation: (Line: 53, Column: 4)));

		[Theory]
		[EmbeddedFileData("DacWithInvalidAggregatorAttributes.cs")]
		public virtual void DacPropertyWithInvalidAggregatorAttributes(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators.CreateFor(line: 41, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators.CreateFor(line: 55, column: 4));

		[Theory]
		[EmbeddedFileData("DacFieldTypeMismatchPXDBScalarAttr.cs")]
		public virtual void DacPropertyWithPXDBScalarAttribute(string source) =>
			VerifyCSharpDiagnostic(source);	
	}
}
