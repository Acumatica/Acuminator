using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InheritanceFromPXCacheExtension
{
    public class InheritanceFromPXCacheExtensionTests : Verification.CodeFixVerifier
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

	    private DiagnosticResult CreatePX1011DiagnosticResult(int line, int column)
	    {
		    var diagnostic = new DiagnosticResult
		    {
			    Id = Descriptors.PX1011_InheritanceFromPXCacheExtension.Id,
			    Message = Descriptors.PX1011_InheritanceFromPXCacheExtension.Title.ToString(),
			    Severity = DiagnosticSeverity.Warning,
			    Locations =
				    new[] {
					    new DiagnosticResultLocation("Test0.cs", line, column)
				    }
		    };

		    return diagnostic;
	    }

		[Theory]
        [EmbeddedFileData("InheritanceFromPXCacheExtension_Good.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

		[Theory]
		[EmbeddedFileData("InheritanceFromPXCacheExtension_Bad.cs")]
		public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual,
	            CreatePX1009DiagnosticResult(10, 15),
				CreatePX1011DiagnosticResult(12, 15),
				CreatePX1011DiagnosticResult(13, 15));
        }

	    [Theory]
	    [EmbeddedFileData("InheritanceFromPXMappedCacheExtension.cs")]
	    public void TestDiagnostic_PXMappedCacheExtension_ShouldNotShowDiagnostic(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
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

	public class InheritanceFromPXCacheExtensionMakeSealedTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("InheritanceFromPXCacheExtensionMakeSealed_Bad.cs", "InheritanceFromPXCacheExtensionMakeSealed_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new InheritanceFromPXCacheExtensionMakeSealedFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new InheritanceFromPXCacheExtensionAnalyzer();
		}
	}
}
