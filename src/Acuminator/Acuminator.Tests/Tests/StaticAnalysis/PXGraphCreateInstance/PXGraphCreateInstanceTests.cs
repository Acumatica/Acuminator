using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
    public class PXGraphCreateInstanceTests : Verification.CodeFixVerifier
    {
	    private DiagnosticResult CreatePX1001DiagnosticResult(int line, int column)
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

	    private DiagnosticResult CreatePX1003DiagnosticResult(int line, int column)
	    {
		    var diagnostic = new DiagnosticResult
		    {
			    Id = Descriptors.PX1003_NonSpecificPXGraphCreateInstance.Id,
			    Message = Descriptors.PX1003_NonSpecificPXGraphCreateInstance.Title.ToString(),
			    Severity = DiagnosticSeverity.Warning,
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
            VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(14, 25));
        }

        [Theory]
        [EmbeddedFileData("PXGraphCreateInstanceField.cs")]
        public void TestDiagnostic_Field(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(12, 43));
        }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceProperty.cs")]
	    public void TestDiagnostic_Property(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(14, 17));
	    }

		[Theory]
		[EmbeddedFileData("NonSpecificPXGraphCreateInstanceMethod.cs")]
	    public void TestDiagnosticNonSpecificPXGraph_Method(string actual)
	    {
			VerifyCSharpDiagnostic(actual, CreatePX1003DiagnosticResult(14, 16));
		}

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceMethod.cs",
						  "PXGraphCreateInstanceMethod_Expected.cs")]
	    public void TestCodeFix_Method(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceField.cs",
						  "PXGraphCreateInstanceField_Expected.cs")]
	    public void TestCodeFix_Field(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceProperty.cs",
						  "PXGraphCreateInstanceProperty_Expected.cs")]
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
