using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacExtensionDefaultAttribute
{
    public class DefaultAttributeInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacExtensionDefaultAttributeAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacExtensionDefaultAttributeFix();

        [Theory]
        [EmbeddedFileData("DacExtensionWithoutPersistingCheckNothingValid.cs")]
        public async Task ValidDacExtensionWithoutPersistingcheckNothing(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData("DacExtensionWithoutPersistingCheckNothingInvalid.cs")]
        public async Task InvalidDacExtensionWithoutPersistingcheckNothing(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(8, 10));

        [Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs")]
		public virtual void DacExtensionWithBoundAttribute(string source) =>
			VerifyCSharpDiagnostic(new[] { source },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 23, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 30, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 44, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 50, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 56, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 62, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs")]
		public virtual void DacExtensionWithUnboundFields(string source) =>
			VerifyCSharpDiagnostic(new[] { source },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 35, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundAndUnboundFields.cs", "SOOrder.cs")]
		public virtual void DacExtensionWithBoundAndUnboundAttribute(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 16, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 35, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs", "SOOrder.cs")]
		public virtual void DacExtensionWithAggregateAttributeFields(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs",
							"AggregateAttributeFields_Expected.cs")]
		public virtual void CodeFix_DacExtensionWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs",
						  "DacExtensionWithBoundFields_Expected.cs")]
		public virtual void CodeFix_DacExtensionWithBoundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs",
						  "DacExtensionWithUnboundFields_Expected.cs")]
		public virtual void CodeFix_DacExtensionWithUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected, 1);

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs")]
		public virtual void DacWithBoundAndUnboundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				 Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData("FullyUnboundDac.cs")]
		public virtual void FullyUnboundDac_ShouldNotShow(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("FullyUnboundDacExtension.cs")]
		public virtual void FullyUnboundDacExtension_ShouldNotShow(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DacAggregateAttributeFields.cs")]
		public virtual void DacWithAggregateAttributeFields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs", "DacWithBoundAndUnboundFields_Expected.cs")]
		public virtual void CodeFix_DacWithBoundAndUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacAggregateAttributeFields.cs", "DacAggregateAttributeFields_Expected.cs")]
		public virtual void CodeFix_DacWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}
