using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacExtensionDefaultAttribute
{
    public class DefaultAttributeInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacExtensionDefaultAttributeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacExtensionDefaultAttributeFix();

        [Theory]
        [EmbeddedFileData("DacExtensionWithoutPersistingCheckNothingValid.cs")]
        public async Task TestValidDacExtensionWithoutPersistingcheckNothing(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData("DacExtensionWithoutPersistingCheckNothingInvalid.cs")]
        public async Task TestInvalidDacExtensionWithoutPersistingcheckNothing(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(8, 10));

        [Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs", "SOOrder.cs")]
		public virtual void TestDacExtensionWithBoundAttribute(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 23, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 30, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 44, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 50, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 56, column: 13),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 62, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs", "SOOrder.cs")]
		public virtual void TestDacExtensionWithUnboundFields(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsError.CreateFor(line: 23, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundAndUnboundFields.cs", "SOOrder.cs")]
		public virtual void TestDacExtensionWithBoundAndUnboundAttribute(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsError.CreateFor(line: 16, column: 4),
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning.CreateFor(line: 35, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs", "SOOrder.cs")]
		public virtual void TestDacExtensionWithAggregateAttributeFields(string source, string additionalSource) =>
			VerifyCSharpDiagnostic(new[] { source, additionalSource },
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsError.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs",
							"AggregateAttributeFields_Expected.cs")]
		public virtual void TestCodeFixDacExtensionWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs",
							"DacExtensionWithBoundFields_Expected.cs")]
		public virtual void TestCodeFixDacExtensionWithBoundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs",
							"DacExtensionWithUnboundFields_Expected.cs")]
		public virtual void TestCodeFixDacExtensionWithUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected, 1);

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs")]
		public virtual void TestDacWithBoundAndUnboundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				 Descriptors.PX1030_DefaultAttibuteToExisitingRecordsOnDAC.CreateFor(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData("DacAggregateAttributeFields.cs")]
		public virtual void TestDacWithAggregateAttributeFields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1030_DefaultAttibuteToExisitingRecordsOnDAC.CreateFor(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs", "DacWithBoundAndUnboundFields_Expected.cs")]
		public virtual void TestCodeFixDacWithBoundAndUnboundAttribute(string actual, string expected) =>
		VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacAggregateAttributeFields.cs", "DacAggregateAttributeFields_Expected.cs")]
		public virtual void TestCodeFixDacWithAggregateAttributeFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}
