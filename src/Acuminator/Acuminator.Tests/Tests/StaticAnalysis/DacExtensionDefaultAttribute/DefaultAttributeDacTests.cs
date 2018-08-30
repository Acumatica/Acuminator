using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacExtensionDefaultAttribute
{
	public class DefaultAttributeInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacExtensionDefaultAttributeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacExtensionDefaultAttributeFix();

		[Theory]
		[EmbeddedFileData("DacExtensionWithBoundFields.cs")]
		public virtual void TestDacExtensionWithBoundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4),
				CreatePX1030DiagnosticResult(line: 44, column: 4),
				CreatePX1030DiagnosticResult(line: 50, column: 13),
				CreatePX1030DiagnosticResult(line: 56, column: 13),
				CreatePX1030DiagnosticResult(line: 62, column: 4));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnboundFields.cs")]
		public virtual void TestDacExtensionWithUnboundFields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4));

		[Theory]
		[EmbeddedFileData("DacWithBoundAndUnboundFields.cs")]
		public virtual void TestDacWithBoundAndUnboundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs")]
		public virtual void TestDacWithAggregateAttributeFields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData("AggregateAttributeFields.cs",
							"AggregateAttributeFields_Expected.cs")]
		public virtual void TestCodeFixDacWithAggregateAttributeFields(string actual, string expected) =>
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
			VerifyCSharpFix(actual, expected);

		private DiagnosticResult CreatePX1030DiagnosticResult(int line, int column)
		{
			return new DiagnosticResult
			{
				Id = Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id,
				Message = Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Title.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};
		}
	}
}
