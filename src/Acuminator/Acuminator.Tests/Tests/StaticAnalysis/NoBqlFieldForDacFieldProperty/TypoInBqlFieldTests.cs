#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoBqlFieldForDacFieldProperty
{
	public class TypoInBqlFieldTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithSingleWrongBqlFields.cs")]
		public async Task RegularDac_SingleBqlField_WithTypo(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1066_TypoInBqlFieldName.CreateFor(10, 25, "NoteID"));

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithMultipleWrongBqlFields.cs")]
		public async Task RegularDac_MultipleBqlField_WithTypo(string actual) => 
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(10, 25, "NoteID"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(10, 25, "NoteID1"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(15, 25, "NoteID"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(15, 25, "NoteID1"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(17, 25, "NoteID"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(17, 25, "NoteID1"));

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac.cs")]
		public async Task DerivedDac_TypoInHidingBqlField_FromBaseDac(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1066_TypoInBqlFieldName.CreateFor(10, 29, "Status"));

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithTyposInBqlField.cs")]
		public async Task DacExtension_BqlFieldsWithTypos_InExtension_AndBasedDac(string actual) => 
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(14, 17, "Status"),
				Descriptors.PX1066_TypoInBqlFieldName.CreateFor(17, 25, "Selected"));

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithSingleWrongBqlFields.cs", @"TypoInBqlField\DacWithSingleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_SingleBqlField_Rename(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithMultipleWrongBqlFields.cs", @"TypoInBqlField\DacWithMultipleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_MultipleBqlField_Rename(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac.cs", "DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task TypoInBqlFieldInBaseDac_AddBqlFields_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithTyposInBqlField.cs", @"TypoInBqlField\DacExtensionWithTyposInBqlField_Expected.cs")]
		public async Task DacExtension_BqlFieldsWithTypos_InExtension_AndBasedDac_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithSingleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_SingleBqlField_AfterRename_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithMultipleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_MultipleBqlField_AfterRename_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task DerivedDac_BqlFieldInBaseDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithTyposInBqlField_Expected.cs")]
		public async Task DacExtension_BqlFieldsWithTypos_InExtension_AndBasedDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(14, 17, "Status"));

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NoBqlFieldForDacFieldPropertyAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInBqlFieldFix();

		protected override int? GetAttemptsCount(int initialDiagnosticCount) => null;
	}
}