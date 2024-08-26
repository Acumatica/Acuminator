
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
	public class NoBqlFieldForDacFieldPropertyTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("DacWithoutBqlFields.cs")]
		public async Task RegularDac_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(13, 16, "BoolField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(16, 19, "BoolField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(19, 16, "ByteField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(22, 16, "ByteField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(25, 17, "ShortField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(28, 17, "ShortField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(31, 17, "IntField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(34, 15, "IntField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(37, 16, "LongField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(40, 17, "LongField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(43, 18, "FloatField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(46, 17, "FloatField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(49, 18, "DoubleField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(52, 18, "DoubleField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(55, 19, "DecimalField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(59, 19, "DecimalField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(62, 17, "StringField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(66, 17, "StringField2"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(70, 20, "DateField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(74, 16, "GuidField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(77, 17, "BinaryField"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(81, 17, "BinaryField2"));

		[Theory]
		[EmbeddedFileData("DacWithBqlFieldMissingInBaseDac.cs")]
		public async Task DerivedDac_BasedDac_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(8, 15, "ShipmentNbr"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(23, 23, "ShipmentNbr"));

		[Theory]
		[EmbeddedFileData("DacExtensionWithBqlFieldMissingInBaseDac.cs")]
		public async Task DacExtension_BasedDac_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(9, 22, "ShipmentNbr"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(18, 16, "Selected"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(34, 23, "ShipmentNbr"));

		[Theory]
		[EmbeddedFileData("DacWithBqlFieldMissingInExternalBaseDac.cs")]
		public async Task DerivedDac_BasedDacInExternalDll_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(8, 15, "ShipmentNbr"),
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(12, 25, "OrderNbr"));

		[Theory]
		[EmbeddedFileData("DacExtensionWithBqlFieldMissingInExternalBaseDac.cs")]
		public async Task DacExtension_BasedDacInExternalDll_WithoutBqlFields(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1065_NoBqlFieldForDacFieldProperty.CreateFor(11, 22, "ShipmentNbr"));

		[Theory]
		[EmbeddedFileData("DacWithoutBqlFields.cs", "DacWithoutBqlFields_Expected.cs")]
		public async Task RegularDac_AddBqlFields_CodeFix(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithBqlFieldMissingInBaseDac.cs", "DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task MissingBqlFieldInBaseDac_AddBqlFields_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithBqlFieldMissingInBaseDac.cs", "DacExtensionWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task MissingBqlFieldInBaseDacExtension_AddBqlFields_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithoutBqlFields_Expected.cs")]
		public async Task RegularDac_WithBqlFields_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("DacWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task DerivedDac_BqlFieldInBaseDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("DacExtensionWithBqlFieldMissingInBaseDac_Expected.cs")]
		public async Task DacExtension_BqlFieldInBaseDac_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty.NoBqlFieldForDacFieldPropertyAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NoBqlFieldForDacFieldAnalyzerFix();

		protected override int? GetAttemptsCount(int initialDiagnosticCount) => null;
	}
}