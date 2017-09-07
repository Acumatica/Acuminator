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
    public class PXGraphCreateInstanceTests : CodeFixVerifier
    {
	    private DiagnosticResult CreateDiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1001_PXGraphCreateInstance.Id,
				Message = Descriptors.PX1001_PXGraphCreateInstance.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

        [Theory]
        [EmbeddedFileData("PXGraphCreateInstanceMethod.cs")]
        public void TestDiagnostic_Method(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResult(14, 25));
        }

        [Theory]
        [EmbeddedFileData("PXGraphCreateInstanceField.cs")]
        public void TestDiagnostic_Field(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResult(12, 43));
        }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceProperty.cs")]
	    public void TestDiagnostic_Property(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, CreateDiagnosticResult(14, 17));
	    }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceMethod.cs", "PXGraphCreateInstanceMethod_Expected.cs")]
	    public void TestCodeFix_Method(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceField.cs", "PXGraphCreateInstanceField_Expected.cs")]
	    public void TestCodeFix_Field(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceProperty.cs", "PXGraphCreateInstanceProperty_Expected.cs")]
	    public void TestCodeFix_Property(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new PXGraphCreateInstanceFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PXGraphCreateInstanceAnalyzer();
        }
    }
}
