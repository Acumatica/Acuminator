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
				Descriptors.PX1036_WrongDacSingleUniqueKeyName.CreateFor(
					location: (Line: 14, Column: 16),
					extraLocation: (Line: 6, Column: 2)));

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_AllNotInUK.cs")]
		public async Task Dac_MultipleUniqueKeys_AllNotInUK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(
					location: (Line: 11, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 8,  Column: 2),
						(Line: 27, Column: 17),
						(Line: 33, Column: 16)
					}),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(
					location: (Line: 27, Column: 17),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 8,  Column: 2),
						(Line: 11, Column: 16),
						(Line: 33, Column: 16)
					}),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(
					location: (Line: 33, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 8,  Column: 2),
						(Line: 11, Column: 16),
						(Line: 27, Column: 17)
					}));

		[Theory]
		[EmbeddedFileData(@"WrongUniqueKeyDeclaration\INUnit_MultipleKeys_SomeNotInUK.cs")]
		public async Task Dac_MultipleUniqueKeys_SomeNotInUK(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(
					location: (Line: 30, Column: 17),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 8,  Column: 2),
						(Line: 36, Column: 16)
					}),
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.CreateFor(
					location: (Line: 36, Column: 16),
					extraLocations: new (int Line, int Column)[]
					{
						(Line: 8,  Column: 2),
						(Line: 30, Column: 17)
					}));

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
