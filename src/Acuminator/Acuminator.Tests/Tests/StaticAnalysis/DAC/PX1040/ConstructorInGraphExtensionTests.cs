using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class ConstructorInGraphExtensionTests : CodeFixVerifier
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
		[EmbeddedFileData(@"Dac\PX1040\Diagnostics\ConstructorInGraphExtension.cs")]
		public void TestDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1040DiagnosticResult(18, 10));
		}

		[Theory]
		[EmbeddedFileData(@"Dac\PX1040\Diagnostics\ConstructorInGraphExtensionWithInitialize.cs")]
		public void TestDiagnostic_WithInitialize(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1040DiagnosticResult(18, 10));
		}

		[Theory]
		[EmbeddedFileData(@"Dac\PX1040\CodeFixes\ConstructorInGraphExtension_Expected.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Dac\PX1040\CodeFixes\ConstructorInGraphExtensionWithInitialize_Expected.cs")]
		public void TestDiagnostic_WithInitialize_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Dac\PX1040\Diagnostics\ConstructorInGraphExtension.cs",
			@"Dac\PX1040\CodeFixes\ConstructorInGraphExtension_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"Dac\PX1040\Diagnostics\ConstructorInGraphExtensionWithInitialize.cs",
			@"Dac\PX1040\CodeFixes\ConstructorInGraphExtensionWithInitialize_Expected.cs")]
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
