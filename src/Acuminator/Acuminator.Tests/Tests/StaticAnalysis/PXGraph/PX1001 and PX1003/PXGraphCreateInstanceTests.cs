using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using CodeFixVerifier = Acuminator.Tests.Verification.CodeFixVerifier;

namespace Acuminator.Tests
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
        [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceMethod.cs")] 
		public void TestDiagnostic_Method(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(14, 25));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceField.cs")]
        public void TestDiagnostic_Field(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(12, 43));
        }

	    [Theory]
	    [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceProperty.cs")]
	    public void TestDiagnostic_Property(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, CreatePX1001DiagnosticResult(14, 17));
	    }

		[Theory]
		[EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\NonSpecificPXGraphCreateInstanceMethod.cs")]
	    public void TestDiagnosticNonSpecificPXGraph_Method(string actual)
	    {
			VerifyCSharpDiagnostic(actual, CreatePX1003DiagnosticResult(14, 16));
		}

	    [Theory]
	    [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceMethod.cs",
						  @"PXGraph\PX1001 and PX1003\CodeFixes\PXGraphCreateInstanceMethod_Expected.cs")]
	    public void TestCodeFix_Method(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceField.cs",
						  @"PXGraph\PX1001 and PX1003\CodeFixes\PXGraphCreateInstanceField_Expected.cs")]
	    public void TestCodeFix_Field(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

	    [Theory]
	    [EmbeddedFileData(@"PXGraph\PX1001 and PX1003\Diagnostics\PXGraphCreateInstanceProperty.cs",
						  @"PXGraph\PX1001 and PX1003\CodeFixes\PXGraphCreateInstanceProperty_Expected.cs")]
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
