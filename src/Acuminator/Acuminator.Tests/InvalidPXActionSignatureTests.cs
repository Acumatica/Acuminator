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
	public class InvalidPXActionSignatureTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs")]
		public void TestDiagnostic(string actual)
		{
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1000_InvalidPXActionHandlerSignature.Id,
				Message = Descriptors.PX1000_InvalidPXActionHandlerSignature.Title.ToString(),
				Severity = Descriptors.PX1000_InvalidPXActionHandlerSignature.DefaultSeverity,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 17, 21)
					}
			};

			VerifyCSharpDiagnostic(actual, diagnostic);
		}

	    [Theory]
	    [EmbeddedFileData("InvalidPXActionSignature.cs", "InvalidPXActionSignature_Expected.cs")]
        public void TestCodeFix(string actual, string expected)
	    {
	        VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature_ValidParameters.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new CorrectPXActionSignatureFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new InvalidPXActionSignatureAnalyzer();
		}
	}
}
