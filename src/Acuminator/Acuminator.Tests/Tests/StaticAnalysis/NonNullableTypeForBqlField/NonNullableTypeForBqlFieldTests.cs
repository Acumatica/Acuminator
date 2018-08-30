using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonNullableTypeForBqlField
{
    public class NonNullableTypeForBqlFieldTests : CodeFixVerifier
    {
	    private DiagnosticResult CreatePX1014DiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1014_NonNullableTypeForBqlField.Id,
				Message = Descriptors.PX1014_NonNullableTypeForBqlField.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

	    [Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField.cs")] 
		public void TestDiagnostic(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, CreatePX1014DiagnosticResult(16, 14));
	    }

		[Theory]
        [EmbeddedFileData("NonNullableTypeForBqlField_Expected.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

	    [Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField_Valid.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic2(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField.cs",
						  "NonNullableTypeForBqlField_Expected.cs")]
	    public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new NonNullableTypeForBqlFieldFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonNullableTypeForBqlFieldAnalyzer();
        }
    }
}
