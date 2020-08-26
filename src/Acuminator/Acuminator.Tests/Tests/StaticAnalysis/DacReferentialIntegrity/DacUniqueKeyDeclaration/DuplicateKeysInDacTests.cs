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
	public class DuplicateKeysInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DuplicateKeysInDacFix();

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\Dac_DuplicateKeys.cs")]
		public async Task Dac_DuplicateUniqueKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location: (Line: 9, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 14, Column: 16),
						(Line: 19, Column: 16)
					}),

				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location: (Line: 14, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 9, Column: 16),
						(Line: 19, Column: 16)
					}),

				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location: (Line: 19, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 9, Column: 16),
						(Line: 14, Column: 16)
					}));

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\Dac_DuplicateKeys.cs", @"DuplicateKeys\Dac_DuplicateKeys_Expected.cs")]
		public async Task DeleteOtherDuplicateKeys_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
