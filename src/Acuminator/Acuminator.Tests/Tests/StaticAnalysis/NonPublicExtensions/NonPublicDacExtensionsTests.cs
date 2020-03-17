using System.Threading.Tasks;
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
		[EmbeddedFileData("NonPublicDacExtension.cs")]
		public async Task NonPublicDacExtensions(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(10, 2),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(20, 15),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(35, 24),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(47, 4));

		[Theory]
        [EmbeddedFileData("NonPublicDacExtension_Expected.cs")]
        public async Task NonPublicDacExtensions_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData("NonPublicDacExtension.cs",
						  "NonPublicDacExtension_Expected.cs")]
	    public async Task NonPublicDacExtensions_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
