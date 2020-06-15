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
	public class DacPrimaryKeyDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("Dac_Without_PrimaryKey.cs")]
		public async Task Dac_WithoutPrimaryKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName.cs")]
		public async Task Dac_WrongPrimaryKeyName(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor(7, 16));

		[Theory]
		[EmbeddedFileData("Dac_MultiplePrimaryKeys.cs")]
		public async Task Dac_MultiplePrimaryKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(9, 16),
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(14, 16),
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(19, 16));

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

		//[Theory]
		//[EmbeddedFileData(
		//	"Dac_Bad.cs",
		//	"Dac_Good_Hidden.cs")]
		//public async Task AddPXHiddenAttribute_VerifyCodeFix(string actual, string expected) =>
		//	await VerifyCSharpFixAsync(actual, expected, 0);

		//[Theory]
		//[EmbeddedFileData(
		//	"Dac_Bad.cs",
		//	"Dac_Good_CacheName.cs")]
		//public async Task AddPXCacheNameAttribute_VerifyCodeFix(string actual, string expected) =>
		//	await VerifyCSharpFixAsync(actual, expected, 1);
	}
}
