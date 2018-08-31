using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature
{
	public class InvalidPXActionSignatureTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph(string actual) =>
			VerifyCSharpDiagnostic(actual, CreatePX100DiagnosticResult(line: 17, column: 15));

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph_Extension(string actual) =>
			VerifyCSharpDiagnostic(actual, CreatePX100DiagnosticResult(line: 35, column: 15));

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs",
						  "InvalidPXActionSignature_Expected.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension.cs",
						  "InvalidPXActionSignatureGraphExtension_Expected.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph_Extension_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature_ValidParameters.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new InvalidPXActionSignatureFix();	

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new InvalidPXActionSignatureAnalyzer();

		private DiagnosticResult CreatePX100DiagnosticResult(int line, int column) =>
			new DiagnosticResult
			{
				Id = Descriptors.PX1000_InvalidPXActionHandlerSignature.Id,
				Message = Descriptors.PX1000_InvalidPXActionHandlerSignature.Title.ToString(),
				Severity = Descriptors.PX1000_InvalidPXActionHandlerSignature.DefaultSeverity,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};
	}
}
