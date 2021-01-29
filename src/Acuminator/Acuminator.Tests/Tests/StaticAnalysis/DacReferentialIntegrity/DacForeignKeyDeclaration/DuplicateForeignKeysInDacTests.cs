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
	public class DuplicateForeignKeysInDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacForeignKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DuplicateKeysInDacFix();

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\PivotFieldNoDuplicateFK.cs")]
		public async Task Dac_WithFKsForTheSameDacField_ButDifferentForeignDacs_NoDuplicates(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\SOLineWithDuplicateInventoryFK.cs")]
		public async Task Dac_WithFkViaPk_AndSimpleFK_DuplicateUniqueKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location:	   (Line: 17, Column: 17), 
					extraLocation: (Line: 19, Column: 17)),

				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location:	   (Line: 19, Column: 17),
					extraLocation: (Line: 17, Column: 17)));

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\SOLineWithDuplicateInventoryFK.cs", @"DuplicateKeys\SOLineWithDuplicateInventoryFK_Expected.cs")]
		public async Task Dac_WithFkViaPk_AndSimpleFK_DeleteDuplicateKeys_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\SOLineWithDuplicateSOOrderFK.cs")]
		public async Task Dac_WithCompositeFK_DuplicateUniqueKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location:	   (Line: 17, Column: 17),
					extraLocation: (Line: 24, Column: 17)),

				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.CreateFor(
					location:	   (Line: 24, Column: 17),
					extraLocation: (Line: 17, Column: 17)));

		[Theory]
		[EmbeddedFileData(@"DuplicateKeys\SOLineWithDuplicateSOOrderFK.cs", @"DuplicateKeys\SOLineWithDuplicateSOOrderFK_Expected.cs")]
		public async Task Dac_WithCompositeFK_DeleteDuplicateKeys_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
