using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.NonPublicExtensions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicExtensions
{
    public class NonPublicGraphExtensionsTests : CodeFixVerifier
    {
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonPublicExtensionFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NonPublicGraphAndDacExtensionAnalyzer());

		[Theory]
		[EmbeddedFileData("NonPublicGraphExtension.cs")]
		public async Task NonPublicGraphExtensions(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(10, 2),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(18, 8),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(30, 24),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(38, 4));

		[Theory]
        [EmbeddedFileData("NonPublicGraphExtension_Expected.cs")]
        public async Task NonPublicGraphExtensions_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData("NonPublicGraphExtension.cs",
						  "NonPublicGraphExtension_Expected.cs")]
	    public async Task NonPublicGraphExtensions_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
