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
	public class DacWithWrongForeignKeyDeclarationsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacForeignKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncorrectDeclarationOfDacKeyFix();

		[Theory]
		[EmbeddedFileData(@"WrongForeignKeyDeclaration\SOLineWithoutKeysContainer.cs")]
		public async Task OneKeyInDac_OneKeyInWrongContainer_NoFKClass(string source)
		{
			var dacLocation = (Line: 7, Column: 23);
			var firstKeyLocation = (Line: 17, Column: 17);
			var secondKeyLocation = (Line: 25, Column: 16);

			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacForeignKeyDeclaration.CreateFor(location: firstKeyLocation, 
																		   extraLocations: new[] { dacLocation, secondKeyLocation }),
				Descriptors.PX1036_WrongDacForeignKeyDeclaration.CreateFor(location: secondKeyLocation,
																		   extraLocations: new[] { dacLocation, firstKeyLocation }));
		}

		[Theory]
		[EmbeddedFileData(@"WrongForeignKeyDeclaration\SOLineWrongDeclarationAndFkClass.cs")]
		public async Task Dac_MultipleUniqueKeys_SomeNotInUK(string source)
		{
			var dacLocation = (Line: 7, Column: 23);
			var keyLocation = (Line: 25, Column: 16);

			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacForeignKeyDeclaration.CreateFor(location: keyLocation, extraLocation: dacLocation));
		}

		[Theory]
		[EmbeddedFileData(@"WrongForeignKeyDeclaration\SOLineWithoutKeysContainer.cs",
						  @"WrongForeignKeyDeclaration\SOLineWithoutKeysContainer_Expected.cs")]
		public async Task PlaceAllForeignKeysIntoNewFK_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"WrongForeignKeyDeclaration\SOLineWrongDeclarationAndFkClass.cs",
						  @"WrongForeignKeyDeclaration\SOLineWrongDeclarationAndFkClass_Expected.cs")]
		public async Task PlaceForeignKeyIntoExistingFK_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
