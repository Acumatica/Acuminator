#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
    public class NonPublicDacAndDacExtensionsTests : CodeFixVerifier
    {
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonPublicDacGraphAndExtensionFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NonPublicGraphAndDacAndExtensionsAnalyzer());

		[Theory]
		[EmbeddedFileData("NonPublicDacExtension.cs")]
		public async Task NonPublicDacExtensions(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(10, 2),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(20, 15),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(35, 24),
				 Descriptors.PX1022_NonPublicDacExtension.CreateFor(47, 4));

		[Theory]
		[EmbeddedFileData("NonPublicDac.cs")]
		public async Task NonPublicDacs(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicDac.CreateFor(6, 2),
				 Descriptors.PX1022_NonPublicDac.CreateFor(16, 15),
				 Descriptors.PX1022_NonPublicDac.CreateFor(31, 17),
				 Descriptors.PX1022_NonPublicDac.CreateFor(43, 4));

		[Theory]
        [EmbeddedFileData("NonPublicDacExtension_Expected.cs")]
        public async Task NonPublicDacExtensions_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("NonPublicDac_Expected.cs")]
		public async Task NonPublicDac_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData("NonPublicDacExtension.cs",
						  "NonPublicDacExtension_Expected.cs")]
	    public async Task NonPublicDacExtensions_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("NonPublicDac.cs",
						  "NonPublicDac_Expected.cs")]
		public async Task NonPublicDacs_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
