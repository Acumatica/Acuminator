
#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldTest : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("LegacyBqlFieldGood.cs")]
		public async Task TestDiagnostic_Good(string actual) => await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("LegacyBqlFieldBad.cs")]
		public async Task TestDiagnostic_Bad(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1060_LegacyBqlField.CreateFor(12, 25, "legacyBoolField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(16, 25, "legacyByteField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(20, 25, "legacyShortField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(24, 25, "legacyIntField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(28, 25, "legacyLongField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(32, 25, "legacyFloatField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(36, 25, "legacyDoubleField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(40, 25, "legacyDecimalField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(44, 25, "legacyStringField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(48, 25, "legacyDateField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(52, 25, "legacyGuidField"),
			Descriptors.PX1060_LegacyBqlField.CreateFor(56, 25, "legacyBinaryField"));

		[Theory]
		[EmbeddedFileData("LegacyBqlFieldBad.cs", "LegacyBqlFieldBad_Expected.cs")]
		public async Task TestCodeFix(string actual, string expected) => await VerifyCSharpFixAsync(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new LegacyBqlFieldAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new LegacyBqlFieldFix();
	}
}