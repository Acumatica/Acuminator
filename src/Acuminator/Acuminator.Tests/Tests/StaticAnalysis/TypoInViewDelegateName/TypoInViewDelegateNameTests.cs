using System;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.TypoInViewDelegateName
{
    public class TypoInViewDelegateNameTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInViewDelegateNameFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			 new PXGraphAnalyzer(CodeAnalysisSettings.Default, 
				 new TypoInViewDelegateNameAnalyzer());

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
            VerifyCSharpDiagnostic(actual,
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 16, column: 22, messageArgs: "Documents"));
        }

	    [Theory]
	    [EmbeddedFileData("TypoInViewDelegateName_Bad.cs",
						  "TypoInViewDelegateName_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		[Theory]
		[EmbeddedFileData("TypoInViewDelegateName_GraphExtension_Bad.cs")]
		public void TestDiagnostic_GraphExtension(string actual)
		{
			VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 25, column: 22, messageArgs: "Documents"));
		}

		[Theory]
		[EmbeddedFileData("TypoInViewDelegateName_GraphExtension_Bad_Expected.cs")]
		public void TestDiagnostic_GraphExtension_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnostic(actual);

		[Theory]
		[EmbeddedFileData("TypoInViewDelegateName_GraphExtension_Bad.cs",
						  "TypoInViewDelegateName_GraphExtension_Bad_Expected.cs")]
		public void TestCodeFix_GraphExtension(string actual, string expected) => VerifyCSharpFix(actual, expected);
	}
}
