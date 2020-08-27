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
	public class DacWithWrongUniqueKeyDeclarationsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncorrectDeclarationOfDacKeyFix();

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\SOOrder_SingleUniqueKey_WrongName.cs")]
		public async Task Dac_SingleUniqueKey_WrongName(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName.CreateFor(14, 16));

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_AllNotInUK.cs")]
		public async Task Dac_MultipleUniqueKeys_AllNotInUK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(11, 16),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(27, 17),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(33, 16));

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_SomeNotInUK.cs")]
		public async Task Dac_MultipleUniqueKeys_SomeNotInUK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(30, 17),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(36, 16));

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\SOOrder_SingleUniqueKey_WrongName.cs", 
						  @"WrongUniqueKeyDeclaration\SOOrder_SingleUniqueKey_WrongName_Expected.cs")]
		public async Task RenameUniqueKeyWithIncorrectName_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_AllNotInUK.cs", 
						  @"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_AllNotInUK_Expected.cs")]
		public async Task PlaceAllUniqueKeysIntoNewUK_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_SomeNotInUK.cs",
						  @"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_SomeNotInUK_Expected.cs")]
		public async Task PlaceUniqueKeysIntoExistingUK_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
