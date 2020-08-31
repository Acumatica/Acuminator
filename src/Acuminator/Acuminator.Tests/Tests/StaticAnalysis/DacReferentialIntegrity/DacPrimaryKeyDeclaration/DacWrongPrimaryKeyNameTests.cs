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
	public class DacWrongPrimaryKeyNameTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new IncorrectDeclarationOfDacKeyFix();

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName.cs")]
		public async Task Dac_WrongPrimaryKeyName(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor((9, 16), extraLocation: (6, 2)));

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName.cs", "Dac_WrongPrimaryKeyName_Expected.cs")]
		public async Task ChangePrimaryKeyName_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
