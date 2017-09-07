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
    public class TypoInViewDelegateNameTests : CodeFixVerifier
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
        [EmbeddedFileData("TypoInViewDelegateName_Good_SameName.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic_SameName(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

	    [Theory]
	    [EmbeddedFileData("TypoInViewDelegateName_Good_DifferentNames.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic_DifferentNames(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
        [EmbeddedFileData("TypoInViewDelegateName_Bad.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1005DiagnosticResult(16, 22));
        }

		//protected override CodeFixProvider GetCSharpCodeFixProvider()
		//{
		//	return new PXGraphCreateInstanceFix();
		//}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypoInViewDelegateNameAnalyzer();
        }
    }
}
