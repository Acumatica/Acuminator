using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Analyzers;
using PX.Analyzers.FixProviders;
using PX.Analyzers.Test.Helpers;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
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
				Severity = DiagnosticSeverity.Error,
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
