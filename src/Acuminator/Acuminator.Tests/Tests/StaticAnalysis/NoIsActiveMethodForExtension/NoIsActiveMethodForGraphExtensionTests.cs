using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension
{
    public class NoIsActiveMethodForGraphExtensionTests : DiagnosticVerifier
    {
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NoIsActiveMethodForExtensionAnalyzer());

		[Theory]
		[EmbeddedFileData("GraphExtension_WithoutIsActive.cs")]
		public async Task GraphExtensions_WithoutIsActive(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1016_NoIsActiveMethodForGraphExtension.CreateFor(10, 22),
				 Descriptors.PX1016_NoIsActiveMethodForGraphExtension.CreateFor(14, 22),
				 Descriptors.PX1016_NoIsActiveMethodForGraphExtension.CreateFor(21, 22),
				 Descriptors.PX1016_NoIsActiveMethodForGraphExtension.CreateFor(26, 22),
				 Descriptors.PX1016_NoIsActiveMethodForGraphExtension.CreateFor(33, 22));

		[Theory]
        [EmbeddedFileData("GraphExtension_WithIsActive.cs")]
        public async Task GraphExtension_WithIsActive_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}
