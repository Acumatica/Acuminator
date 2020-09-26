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
	public class DacUniqueKeysDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("INUnit_MultipleUniqueKeys_UnboundField.cs")]
		public async Task UnboundDacField_InUniqueKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(23, 95),
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(28, 95),
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(36, 98),
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(38, 100));

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
