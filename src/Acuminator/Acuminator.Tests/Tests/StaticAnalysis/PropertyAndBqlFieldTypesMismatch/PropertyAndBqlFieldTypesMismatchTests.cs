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
		public async Task Dac_WithMismatchingTypes_FixBqlFieldType_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithInconsistentTypes_BqlFieldFirst_Expected.cs")]
		public async Task Dac_WithFixedBqlFieldTypes_AfterFix_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}