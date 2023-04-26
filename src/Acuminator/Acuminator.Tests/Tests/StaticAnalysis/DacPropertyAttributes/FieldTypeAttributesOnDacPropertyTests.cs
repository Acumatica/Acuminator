#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class FieldTypeAttributesOnDacPropertyTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPropertyAttributesAnalyzer());

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldTypeAttributes.cs")]
		public virtual Task PropertyWithMultipleFieldTypeAttributes(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 24, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnProperty.CreateFor(line: 25, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithMultipleCalcedOnDbSideAttributes.cs")]
		public virtual Task PropertyWithMultipleCalcedOnDbSideAttributes(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty.CreateFor(line: 16, column: 4),
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty.CreateFor(line: 17, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldTypeAttributes_Expected.cs")]
		public virtual Task MultipleFieldTypeAttributes_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithMultipleCalcedOnDbSideAttributes_Expected.cs")]
		public virtual Task MultipleCalcedOnDbSideAttributes_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch_Expected.cs")]
		public virtual Task DacPropertyTypeNotMatchingAttributeType_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);	

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch.cs")]
		public virtual Task DacPropertyTypeNotMatchingAttributeType(string source) =>
			VerifyCSharpDiagnosticAsync(source,
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
		public virtual Task DacPropertyWithInvalidAggregatorAttributes(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators.CreateFor(line: 41, column: 4),
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators.CreateFor(line: 55, column: 4));

		[Theory]
		[EmbeddedFileData("DacFieldTypeMismatchPXDBScalarAttr.cs")]
		public virtual Task DacPropertyWithPXDBScalarAttribute(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithValidAggregatorAttributes.cs")]
		public virtual Task DacWithValidAggregatorAttributes(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacFieldWithINUnitAttribute.cs")]
		public virtual Task DacFieldWithINUnitAttribute(string source) =>
			VerifyCSharpDiagnosticAsync(source);
	}
}
