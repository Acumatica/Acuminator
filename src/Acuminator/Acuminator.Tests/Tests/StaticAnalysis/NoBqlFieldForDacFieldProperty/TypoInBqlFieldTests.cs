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

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac.cs")]
		//public async Task DerivedDac_BasedDac_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(8, 15, "ShipmentNbr"),
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(23, 23, "ShipmentNbr"));

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithBqlFieldMissingInBaseDac.cs")]
		//public async Task DacExtension_BasedDac_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(9, 22, "ShipmentNbr"),
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(18, 16, "Selected"),
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(34, 23, "ShipmentNbr"));

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInExternalBaseDac.cs")]
		//public async Task DerivedDac_BasedDacInExternalDll_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(8, 15, "ShipmentNbr"),
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(12, 25, "OrderNbr"));

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithBqlFieldMissingInExternalBaseDac.cs")]
		//public async Task DacExtension_BasedDacInExternalDll_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
		//	Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(11, 22, "ShipmentNbr"));

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithSingleWrongBqlFields.cs", @"TypoInBqlField\DacWithSingleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_RenameSingleBqlField_CodeFix(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac.cs", "DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		//public async Task TypoInBqlFieldInBaseDac_AddBqlFields_CodeFix(string actual, string expected) =>
		//	await VerifyCSharpFixAsync(actual, expected);

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithBqlFieldMissingInBaseDac.cs", "DacExtensionWithBqlFieldMissingInBaseDac_Expected.cs")]
		//public async Task TypoInBqlFieldInBaseDacExtension_AddBqlFields_CodeFix(string actual, string expected) =>
		//	await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"TypoInBqlField\DacWithSingleWrongBqlFields_Expected.cs")]
		public async Task RegularDac_WithBqlFields_AfterRename_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		//public async Task DerivedDac_BqlFieldInBaseDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
		//	await VerifyCSharpDiagnosticAsync(actual);

		//[Theory]
		//[EmbeddedFileData(@"TypoInBqlField\DacExtensionWithBqlFieldMissingInBaseDac_Expected.cs")]
		//public async Task DacExtension_BqlFieldInBaseDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
		//	await VerifyCSharpDiagnosticAsync(actual);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NoBqlFieldForDacFieldPropertyAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInBqlFieldFix();

		protected override int? GetAttemptsCount(int initialDiagnosticCount) => null;
	}
}