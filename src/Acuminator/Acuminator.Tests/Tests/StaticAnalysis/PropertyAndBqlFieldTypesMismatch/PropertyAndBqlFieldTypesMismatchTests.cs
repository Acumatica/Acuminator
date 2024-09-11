#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PropertyAndBqlFieldTypesMismatch
{
	public class PropertyAndBqlFieldTypesMismatchTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new PropertyAndBqlFieldTypesMismatchAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PropertyAndBqlFieldTypesMismatchFix();

		#region BQL field first
		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_BqlFieldFirst.cs")]
		public async Task Dac_WithMismatchingTypes_BqlFieldFirst(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(12, 46),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(15, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(19, 34),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(22, 18));

		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_BqlFieldFirst.cs", "DacWithInconsistentTypes_BqlFieldFirst_Expected.cs")]
		public async Task Dac_WithMismatchingTypes_FixBqlFieldType(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_BqlFieldFirst_Expected.cs")]
		public async Task Dac_WithFixedBqlFieldTypes_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion

		#region Property first
		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_PropertyFirst.cs")]
		public async Task Dac_WithMismatchingTypes_PropertyFirst(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(13, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(15, 46),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(20, 18),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(22, 34),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(27, 18),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(29, 46));

		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_PropertyFirst.cs", "DacWithInconsistentTypes_PropertyFirst_Expected.cs")]
		public async Task Dac_WithMismatchingTypes_FixPropertyType(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_PropertyFirst_Expected.cs")]
		public async Task Dac_WithFixedPropertyTypes_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion

		#region DAC Extension
		[Theory]
		[EmbeddedFileData("DacExtensionWithInconsistentTypes.cs")]
		public async Task DacExtension_WithMismatchingTypes(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(13, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(15, 44),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(19, 37),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(22, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(26, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(28, 34));

		[Theory]
		[EmbeddedFileData("DacExtensionWithInconsistentTypes.cs", "DacExtensionWithInconsistentTypes_Expected.cs")]
		public async Task DacExtension_WithMismatchingTypes_FixTypes(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithInconsistentTypes_Expected.cs")]
		public async Task DacExtension_WithFixedTypes_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion

		#region Derived DAC
		[Theory]
		[EmbeddedFileData("DerivedDacWithInconsistentTypes.cs")]
		public async Task DerivedDac_WithMismatchingTypes(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(13, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(15, 44),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(19, 37),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(22, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(26, 10),
				Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.CreateFor(28, 34));

		[Theory]
		[EmbeddedFileData("DerivedDacWithInconsistentTypes.cs", "DerivedDacWithInconsistentTypes_Expected.cs")]
		public async Task DerivedDac_WithMismatchingTypes_FixTypes(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DerivedDacWithInconsistentTypes_Expected.cs")]
		public async Task DerivedDac_WithFixedTypes_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
		#endregion
	}
}