using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity
{
	public class DacPrimaryKeyDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("Dac_GoodPrimaryKey.cs")]
		public async Task Dac_WithCorrectPrimaryKey_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("PXMappedCacheExtension.cs")]
		public async Task PXMappedCacheExtension_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Fully_Unbound_Dac.cs")]
		public async Task FullyUnboundDac_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
	}
}
