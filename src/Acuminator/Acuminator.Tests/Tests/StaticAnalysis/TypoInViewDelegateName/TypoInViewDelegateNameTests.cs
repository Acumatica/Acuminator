using System;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.TypoInViewDelegateName
{
    public class TypoInViewDelegateNameTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInViewDelegateNameFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new TypoInViewDelegateNameAnalyzer();


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
	    [EmbeddedFileData("TypoInViewDelegateName_Good_Override.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic_Override(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
        [EmbeddedFileData("TypoInViewDelegateName_Bad.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, Descriptors.PX1005_TypoInViewDelegateName.CreateFor(16, 22, "Documents"));
        }

	    [Theory]
	    [EmbeddedFileData("TypoInViewDelegateName_Bad.cs",
						  "TypoInViewDelegateName_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }
    }
}
