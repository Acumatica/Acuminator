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
	public class MultiplePrimaryKeysInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new MultiplePrimaryKeysInDacFix();

		[Theory]
		[EmbeddedFileData("Dac_MultiplePrimaryKeys.cs")]
		public async Task Dac_MultiplePrimaryKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(
					location: (Line: 9, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 14, Column: 16),
						(Line: 19, Column: 16)
					}),

				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(
					location: (Line: 14, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 9, Column: 16),
						(Line: 19, Column: 16)
					}),

				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac.CreateFor(
					location: (Line: 19, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 9, Column: 16),
						(Line: 14, Column: 16)
					}));

		[Theory]
		[EmbeddedFileData("Dac_MultiplePrimaryKeys.cs", "Dac_MultiplePrimaryKeys_Expected.cs")]
		public async Task DeleteOtherPrimaryKeyDeclarations_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
