using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class DacFieldWithDBCalcedAttributeTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacPropertyAttributesAnalyzer();

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBCalcedAndNonDBAttribute_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndWithoutUnboundTypeAttributes.cs")]
		public async Task DacFieldWithPXDBCalcedAndWithoutNonDBAttribute_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(19, 28));
	}
}
