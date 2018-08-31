using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ConstructorInGraphExtension
{
	public class ConstructorInGraphExtensionTests : Verification.CodeFixVerifier
	{
		private DiagnosticResult CreatePX1040DiagnosticResult(int line, int column)
		{
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1040_ConstructorInGraphExtension.Id,
				Message = Descriptors.PX1040_ConstructorInGraphExtension.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

			return diagnostic;
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension.cs")]
		public void TestDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1040DiagnosticResult(18, 10));
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize.cs")]
		public void TestDiagnostic_WithInitialize(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1040DiagnosticResult(18, 10));
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension_Expected.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize_Expected.cs")]
		public void TestDiagnostic_WithInitialize_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension.cs",
			"ConstructorInGraphExtension_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize.cs",
			"ConstructorInGraphExtensionWithInitialize_Expected.cs")]
		public void TestCodeFix_WithInitialize(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new ConstructorInGraphExtensionCodeFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new ConstructorInGraphExtensionAnalyzer();
		}
	}
}
