using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using CodeFixVerifier = Acuminator.Tests.Verification.CodeFixVerifier;

namespace Acuminator.Tests
{
    public class TypoInViewDelegateNameTests : Verification.CodeFixVerifier
    {
	    private DiagnosticResult CreatePX1005DiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1005_TypoInViewDelegateName.Id,
				Message = String.Format(Descriptors.PX1005_TypoInViewDelegateName.MessageFormat.ToString(), "Documents"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

		[Theory]
        [EmbeddedFileData(@"View\Delegate\PX1005\Diagnostics\TypoInViewDelegateName_Good_SameName.cs")] 
		public void TestDiagnostic_ShouldNotShowDiagnostic_SameName(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

	    [Theory]
	    [EmbeddedFileData(@"View\Delegate\PX1005\Diagnostics\TypoInViewDelegateName_Good_DifferentNames.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic_DifferentNames(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

	    [Theory]
	    [EmbeddedFileData(@"View\Delegate\PX1005\Diagnostics\TypoInViewDelegateName_Good_Override.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic_Override(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
        [EmbeddedFileData(@"View\Delegate\PX1005\Diagnostics\TypoInViewDelegateName_Bad.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1005DiagnosticResult(16, 22));
        }

	    [Theory]
	    [EmbeddedFileData(@"View\Delegate\PX1005\Diagnostics\TypoInViewDelegateName_Bad.cs",
						  @"View\Delegate\PX1005\CodeFixes\TypoInViewDelegateName_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new TypoInViewDelegateNameFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypoInViewDelegateNameAnalyzer();
        }
    }
}
