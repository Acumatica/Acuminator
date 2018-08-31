using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
    public class PXGraphCreateInstanceTests : CodeFixVerifier
    {
		[Theory]
        [EmbeddedFileData("PXGraphCreateInstanceMethod.cs")] 
		public void TestDiagnostic_Method(string actual)
        {
            VerifyCSharpDiagnostic(actual, Descriptors.PX1001_PXGraphCreateInstance.CreateFor(14, 25));
        }

        [Theory]
        [EmbeddedFileData("PXGraphCreateInstanceField.cs")]
        public void TestDiagnostic_Field(string actual)
        {
            VerifyCSharpDiagnostic(actual, Descriptors.PX1001_PXGraphCreateInstance.CreateFor(12, 43));
        }

	    [Theory]
	    [EmbeddedFileData("PXGraphCreateInstanceProperty.cs")]
	    public void TestDiagnostic_Property(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, Descriptors.PX1001_PXGraphCreateInstance.CreateFor(14, 17));
	    }

		[Theory]
		[EmbeddedFileData("NonSpecificPXGraphCreateInstanceMethod.cs")]
	    public void TestDiagnosticNonSpecificPXGraph_Method(string actual)
	    {
			VerifyCSharpDiagnostic(actual, Descriptors.PX1003_NonSpecificPXGraphCreateInstance.CreateFor(14, 16));
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
