using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NonPublicExtensions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicExtensions
{
    public class NonPublicDacExtensionsTests : CodeFixVerifier
    {
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonPublicExtensionFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NonPublicGraphAndDacExtensionAnalyzer());

		[Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField.cs")] 
		public void TestDiagnostic(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, Descriptors.PX1014_NonNullableTypeForBqlField.CreateFor(16, 14));
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
    }
}
