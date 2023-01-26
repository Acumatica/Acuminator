#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

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
		public virtual Task DacExtensionWithBoundAttribute(string source) =>
			VerifyCSharpDiagnosticAsync(new[] { source },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 23, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 30, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 44, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 50, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 56, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 62, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs")]
		public virtual Task DacExtensionWithUnboundFields(string source) =>
			VerifyCSharpDiagnosticAsync(new[] { source },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 35, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundAndUnboundFields.cs", "SOOrder.cs")]
		public virtual Task DacExtensionWithBoundAndUnboundAttribute(string source, string additionalSource) =>
			VerifyCSharpDiagnosticAsync(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 16, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning.CreateFor(line: 35, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs", "SOOrder.cs")]
		public virtual Task DacExtensionWithAggregateAttributeFields(string source, string additionalSource) =>
			VerifyCSharpDiagnosticAsync(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsError.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs",
						  "AggregateAttributeFields_Expected.cs")]
		public virtual Task CodeFix_DacExtensionWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs",
						  "DacExtensionWithBoundFields_Expected.cs")]
		public virtual Task CodeFix_DacExtensionWithBoundAttribute(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs",
						  "DacExtensionWithUnboundFields_Expected.cs")]
		public virtual Task CodeFix_DacExtensionWithUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 1);

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs")]
		public virtual Task DacWithBoundAndUnboundAttribute(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				 Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData("FullyUnboundDac.cs")]
		public virtual Task FullyUnboundDac_ShouldNotShow(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("FullyUnboundDacExtension.cs")]
		public virtual Task FullyUnboundDacExtension_ShouldNotShow(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithAggregateAttributeOnFields.cs")]
		public virtual Task DacWithAggregateAttributeFields(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs", "DacWithBoundAndUnboundFields_Expected.cs")]
		public virtual Task CodeFix_DacWithBoundAndUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithAggregateAttributeOnFields.cs", "DacWithAggregateAttributeOnFields_Expected.cs")]
		public virtual Task CodeFix_DacWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}
