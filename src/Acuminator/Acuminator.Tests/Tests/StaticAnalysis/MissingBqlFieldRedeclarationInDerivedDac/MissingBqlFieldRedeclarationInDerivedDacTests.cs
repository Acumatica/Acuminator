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
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields.cs")]
		public async Task Dac_WithoutRedeclaredBqlFields_FromBaseDac(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.CreateFor(8, 15, "DerivedDac", "Tstamp", "BaseDac"),
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.CreateFor(8, 15, "DerivedDac", "status", "BaseDac"),
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.CreateFor(8, 15, "DerivedDac", "Tstamp2", "BaseDac"),
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.CreateFor(8, 15, "DerivedDac", "Tstamp3", "BaseDac"),
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.CreateFor(12, 24, "DerivedDac", "shipmentNbr", "BaseDac"));

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields.cs", "DacWithNotRedeclaredBqlFields_Expected.cs")]
		public async Task Dac_RedeclareBqlFields_CodeFix(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_Expected.cs")]
		public async Task Dac_WithRedeclaredBqlFields_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingBqlFieldRedeclarationInDerivedDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingBqlFieldRedeclarationInDerivedDacFix();
	}
}