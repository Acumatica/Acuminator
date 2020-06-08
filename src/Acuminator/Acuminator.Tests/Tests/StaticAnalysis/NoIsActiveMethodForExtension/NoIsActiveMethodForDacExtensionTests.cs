using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension
{
    public class NoIsActiveMethodForDacExtensionTests : DiagnosticVerifier
    {
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NoIsActiveMethodForExtensionAnalyzer());

		[Theory]
		[EmbeddedFileData("DacExtension_WithoutIsActive.cs")]
		public async Task DacExtensions_WithoutIsActive(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1016_NoIsActiveMethodForDacExtension.CreateFor(10, 22),
				 Descriptors.PX1016_NoIsActiveMethodForDacExtension.CreateFor(14, 22),
				 Descriptors.PX1016_NoIsActiveMethodForDacExtension.CreateFor(21, 22),
				 Descriptors.PX1016_NoIsActiveMethodForDacExtension.CreateFor(26, 22),
				 Descriptors.PX1016_NoIsActiveMethodForDacExtension.CreateFor(33, 22));

		[Theory]
		[EmbeddedFileData("DAC_MappedCacheExtension.cs")]
		public async Task MappedCacheExtension_WithoutIsActive_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
        [EmbeddedFileData("DacExtension_WithIsActive.cs")]
        public async Task DacExtension_WithIsActive_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);	
	}
}
