using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity
{
	public class DacForeignKeyDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
		new DacAnalyzersAggregator(
			CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled(),
			new DacForeignKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData(@"CorrectForeignKey\Dac_GoodForeignKey.cs", @"CorrectForeignKey\SOOrder.cs")]
		public async Task Dac_WithCorrectForeignKey_DoesntReportDiagnostic(string testedSource, string soOrderSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { testedSource, soOrderSource });

		[Theory]
		[EmbeddedFileData(@"CorrectForeignKey\Dac_GoodForeignKey_AsSimpleKey.cs", @"CorrectForeignKey\SOOrder.cs")]
		public async Task Dac_WithCorrectForeignKey_UsingAsSimpleKey_DoesntReportDiagnostic(string testedSource, string soOrderSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { testedSource, soOrderSource });

		[Theory]
		[EmbeddedFileData(@"CorrectForeignKey\Dac_GoodForeignKey_CompositeKey.cs", @"CorrectForeignKey\SOOrder.cs")]
		public async Task Dac_WithCorrectForeignKey_UsingCompositeKey_DoesntReportDiagnostic(string testedSource, string soOrderSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { testedSource, soOrderSource });

		[Theory]
		[EmbeddedFileData(@"UnboundFieldInKey\SOLineWithUnboundFieldInCompositeFK.cs", @"UnboundFieldInKey\SOOrder.cs")]
		public async Task UnboundDacField_InCompositeKey(string testedSource, string soOrderSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { testedSource, soOrderSource },
					Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(10, 76),
					Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(17, 100),
					Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(19, 43));

		[Theory]
		[EmbeddedFileData(@"UnboundFieldInKey\SOLineWithUnboundFieldInFK.cs", @"UnboundFieldInKey\SOOrder.cs")]
		public async Task UnboundDacField_SimpleFK_And_FKViaPK(string testedSource, string soOrderSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { testedSource, soOrderSource },
					Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(9, 85),
					Descriptors.PX1037_UnboundDacFieldInKeyDeclaration.CreateFor(17, 23));

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
