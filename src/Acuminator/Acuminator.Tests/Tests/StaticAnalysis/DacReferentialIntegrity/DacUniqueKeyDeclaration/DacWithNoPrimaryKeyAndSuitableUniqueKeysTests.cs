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
	public class DacWithNoPrimaryKeyAndSuitableUniqueKeysTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncorrectDeclarationOfDacKeyFix();

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_OneKey.cs")]
		public async Task Dac_NoPK_SingleUniqueKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor(
					location: (Line: 9, Column: 16), extraLocation: (Line: 6, Column: 2)));

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_TwoKeys.cs")]
		public async Task Dac_NoPK_TwoUniqueKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor(
					location: (Line: 9, Column: 16), extraLocation: (Line: 6, Column: 2)));

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\INUnit_MultipleUniqueKeys_NoPK.cs")]
		public async Task Dac_NoPK_MultipleUniqueKeys(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor(
					location: (Line: 13, Column: 17), extraLocation: (Line: 8, Column: 2)));

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_OneKey.cs", @"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_OneKey_Expected.cs")]
		public async Task TurnUniqueKeyIntoPrimaryKey_SinqleKey_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_TwoKeys.cs", @"TurnUniqueKeyIntoPrimaryKey\SOOrder_NoPK_TwoKeys_Expected.cs")]
		public async Task TurnUniqueKeyIntoPrimaryKey_TwoKeys_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TurnUniqueKeyIntoPrimaryKey\INUnit_MultipleUniqueKeys_NoPK.cs", 
						  @"TurnUniqueKeyIntoPrimaryKey\INUnit_MultipleUniqueKeys_NoPK_Expected.cs")]
		public async Task TurnUniqueKeyIntoPrimaryKey_MultipleUniqueKeys_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
