using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class DacFieldWithDBCalcedAttributeTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPropertyAttributesAnalyzer());

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBCalcedAndNonDBAttribute_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndWithoutUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBCalcedAndWithoutNonDBAttribute_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source, 
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(19, 28),
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(38, 25),
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(48, 25));

		[Theory]
		[EmbeddedFileData("DacWithPXDBScalarAndUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBScalarAndNonDBAttribute_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithPXDBScalarAndWithoutUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBScalarAndWithoutNonDBAttribute_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(20, 23),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(35, 25),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(44, 27),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(53, 25));
	}
}
