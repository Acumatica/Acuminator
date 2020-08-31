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
	public class DacCorrectPrimaryKeyWithUniqueKeysDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("Dac_SingleUniqueKey_Good.cs")]
		public async Task Dac_WithCorrectPrimaryAndSingleUniqueKeys_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Dac_MultipleUniqueKeys_Good.cs")]
		public async Task INUnit_WithCorrectPrimaryAndMultipleUniqueKeys_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
	}
}
