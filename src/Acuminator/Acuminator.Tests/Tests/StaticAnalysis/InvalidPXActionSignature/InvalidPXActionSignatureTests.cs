using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature
{
	public class InvalidPXActionSignatureTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new InvalidPXActionSignatureFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new InvalidPXActionSignatureAnalyzer();

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph(string actual) =>
			VerifyCSharpDiagnostic(actual, Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 17, column: 15));

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension.cs")]
		public void Test_Invalid_Action_Signature_In_PXGraph_Extension(string actual) =>
			VerifyCSharpDiagnostic(actual, Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 35, column: 15));

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
	}
}
