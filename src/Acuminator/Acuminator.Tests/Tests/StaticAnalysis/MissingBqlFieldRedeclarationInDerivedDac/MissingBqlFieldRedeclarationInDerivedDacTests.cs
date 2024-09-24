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
		#region Single Field
		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_SingleField.cs")]
		public async Task Dac_WithoutRedeclaredBqlFields_SingleField(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_SingleField
						   .CreateFor(8, 15, "DerivedDac", "status", "BaseDac"));

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_SingleField.cs", "DacWithNotRedeclaredBqlFields_SingleField_Expected.cs")]
		public async Task Dac_RedeclareBqlFields_SingleField_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_SingleField_Expected.cs")]
		public async Task Dac_WithRedeclaredBqlFields_SingleField_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion

		#region 2 to 5 Fields
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
		#endregion

		#region More than 5 Fields
		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_MoreThan5.cs")]
		public async Task Dac_WithoutRedeclaredBqlFields_MoreThan5_Fields(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_MoreThan5Fields
						   .CreateFor(8, 15, 
									  "DerivedDac", "\"Tstamp\", \"status\", \"Tstamp2\", \"Tstamp3\", \"opportunityIsActive\"", 
									  1, Acuminator.Analyzers.Resources.PX1067MoreThan5Fields_RemainderSingleField));

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_MoreThan5.cs", "DacWithNotRedeclaredBqlFields_MoreThan5_Expected.cs")]
		public async Task Dac_RedeclareBqlFields_MoreThan5_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithNotRedeclaredBqlFields_MoreThan5_Expected.cs")]
		public async Task Dac_WithRedeclaredBqlFields_MoreThan5_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingBqlFieldRedeclarationInDerivedDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingBqlFieldRedeclarationInDerivedDacFix();
	}
}