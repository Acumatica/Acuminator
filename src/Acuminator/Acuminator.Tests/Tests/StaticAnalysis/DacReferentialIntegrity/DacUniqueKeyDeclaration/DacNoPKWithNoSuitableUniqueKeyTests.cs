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
	public class DacNoPKWithNoSuitableUniqueKeyTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacMissingPrimaryKeyFix();

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\INUnit_MultipleUniqueKeys_NoPK_NoSuitableUK.cs")]
		public async Task MultipleUKs_WithoutPrimaryKey_NoUniqueKeySuitableToBePK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.CreateFor(9, 23));

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOOrder_NoPK_And_SingleUK_NotSuitableToBePK.cs")]
		public async Task SingleUK_WithoutPrimaryKey_NoUniqueKeySuitableToBePK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.CreateFor(7, 15));

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\INUnit_MultipleUniqueKeys_NoPK_NoSuitableUK.cs",
						  @"MissingPrimaryKey\INUnit_MultipleUniqueKeys_NoPK_NoSuitableUK_Expected.cs")]
		public async Task AddPrimaryKeyDeclaration_MultipleUKs_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOOrder_NoPK_And_SingleUK_NotSuitableToBePK.cs",
						  @"MissingPrimaryKey\SOOrder_NoPK_And_SingleUK_NotSuitableToBePK_Expected.cs")]
		public async Task AddPrimaryKeyDeclaration_SingleUK_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
