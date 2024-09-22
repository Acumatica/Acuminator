#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingBqlFieldRedeclarationInDerived
{
	public class MissingBqlFieldRedeclarationInDerivedDacTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_From_2_to_5.cs")]
		public async Task Dac_WithoutRedeclaredBqlFields_From_2_To_5_Fields(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_From_2_To_5_Fields
						   .CreateFor(8, 15, "DerivedDac", "\"Tstamp\", \"status\", \"Tstamp2\", \"Tstamp3\", \"shipmentNbr\""));

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_From_2_to_5.cs", "DacWithNotRedeclaredBqlFields_From_2_to_5_Expected.cs")]
		public async Task Dac_RedeclareBqlFields_From_2_To_5_Fields_CodeFix(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_From_2_to_5_Expected.cs")]
		public async Task Dac_WithRedeclaredBqlFields_From_2_To_5_Fields_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingBqlFieldRedeclarationInDerivedDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingBqlFieldRedeclarationInDerivedDacFix();
	}
}