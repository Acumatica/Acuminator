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
	public class DacMissingForeignKeyDeclarationTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacForeignKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new DacMissingForeignKeyFix();

		[Theory]
		[EmbeddedFileData("Dac_Without_ForeignKey.cs")]
		public async Task Dac_WithoutForeignKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1034_MissingDacForeignKeyDeclaration.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData(@"MissingForeignKeyFix\SOOrder_Without_ForeignKey.cs",
						  @"MissingForeignKeyFix\SOOrder_Without_ForeignKey_Expected.cs")]
		public async Task AddForeignKeyTemplateDeclaration_ToSOOrder_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		//[Theory]
		//[EmbeddedFileData(@"MissingPrimaryKey\SOLine_Without_PrimaryKey.cs",
		//				  @"MissingPrimaryKey\SOLine_Without_PrimaryKey_Expected.cs")]
		//public async Task AddPrimaryKeyDeclaration_ToSOLine_VerifyCodeFix(string actual, string expected) =>
		//	await VerifyCSharpFixAsync(actual, expected);
	}
}
