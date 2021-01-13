using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallsToInternalAPI
{
	public class CallsToInternalAPINonIsvModeTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new CallsToInternalAPIAnalyzer(
					CodeAnalysisSettings.Default
										.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled()
										.WithIsvSpecificAnalyzersDisabled());

		[Theory]
		[EmbeddedFileData("InternalAPI.cs")]
		public async Task Field_WithInitializer(string source) => await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("SOShipmentExt.cs", "InternalAPI.cs")]
		public async Task CallsToInternal_Properties_Methods_Fields(string source) => await VerifyCSharpDiagnosticAsync(source);
	}
}
