using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonNullableTypeForBqlField
{
    public class NonNullableTypeForBqlFieldTests : CodeFixVerifier
    {
	    [Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField.cs")] 
		public async Task TestDiagnostic(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual, 
											   Descriptors.PX1014_NonNullableTypeForBqlField.CreateFor(16, 14));

		[Theory]
        [EmbeddedFileData("NonNullableTypeForBqlField_Expected.cs")]
        public async Task TestDiagnostic_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory] 
		[EmbeddedFileData("NonNullableTypeForBqlField_Valid.cs")] 
		public async Task TestDiagnostic_ShouldNotShowDiagnostic2(string actual) => 
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData("NonNullableTypeForBqlField.cs",
						  "NonNullableTypeForBqlField_Expected.cs")]
	    public async Task TestCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new NonNullableTypeForBqlFieldFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NonNullableTypeForBqlFieldAnalyzer());     
    }
}
