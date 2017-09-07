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
    public class InheritanceFromPXCacheExtensionTests : CodeFixVerifier
    {
	    private DiagnosticResult CreatePX1009DiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1009_InheritanceFromPXCacheExtension.Id,
				Message = Descriptors.PX1009_InheritanceFromPXCacheExtension.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

		[Theory]
        [EmbeddedFileData("InheritanceFromPXCacheExtension_Bad_Expected.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

		[Theory]
		[EmbeddedFileData("InheritanceFromPXCacheExtension_Bad.cs")]
		public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1009DiagnosticResult(13, 15));
        }

	    [Theory]
	    [EmbeddedFileData("InheritanceFromPXCacheExtension_Bad.cs", "InheritanceFromPXCacheExtension_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new InheritanceFromPXCacheExtensionFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InheritanceFromPXCacheExtensionAnalyzer();
        }
    }
}
