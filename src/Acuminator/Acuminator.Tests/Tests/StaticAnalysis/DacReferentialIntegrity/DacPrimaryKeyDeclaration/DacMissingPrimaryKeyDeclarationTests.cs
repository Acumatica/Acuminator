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
	public class DacMissingPrimaryKeyDeclarationTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacMissingPrimaryKeyFix();

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOOrder_Without_PrimaryKey.cs")]
		public async Task SOOrder_WithoutPrimaryKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOLine_Without_PrimaryKey.cs")]
		public async Task SOLine_WithoutPrimaryKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOOrder_Without_PrimaryKey.cs", 
						  @"MissingPrimaryKey\SOOrder_Without_PrimaryKey_Expected.cs")]
		public async Task AddPrimaryKeyDeclaration_ToSOOrder_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"MissingPrimaryKey\SOLine_Without_PrimaryKey.cs", 
						  @"MissingPrimaryKey\SOLine_Without_PrimaryKey_Expected.cs")]
		public async Task AddPrimaryKeyDeclaration_ToSOLine_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
