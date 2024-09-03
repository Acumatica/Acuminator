#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BannedApi;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using Resources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.BannedApi
{
	public class BannedApiTestsDefaultFileNonIsv : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new BannedApiAnalyzer(customBannedApiStorage: null, customBannedApiDataProvider: null, 
								  customWhiteListStorage: null, customWhiteListDataProvider: null,
								  customBanInfoRetriever: null, customWhiteListInfoRetriever: null,
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled()
											.WithIsvSpecificAnalyzersDisabled(),
				BannedApiSettings.Default.WithBannedApiAnalysisEnabled()
				);

		[Theory]
		[EmbeddedFileData("CallsToApisForbiddenToIsv.cs")]
		public virtual async Task Calls_To_API_ForbiddenForISV(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("CallsToMathRoundAPIs.cs")]
		public virtual async Task Calls_To_MathRound_API(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(15, 13, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Decimal,System.Int32)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(24, 14, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Decimal)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(34, 13, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Double,System.Int32)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."),
				Descriptors.PX1099_ForbiddenApiUsage_WithReason.CreateFor(43, 18, Resources.PX1099Title_MethodFormatArg, "System.Math.Round(System.Double)",
					"Math.Round uses Bankers Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior. Use Math.Round overload with MidpointRounding parameter to explicitly specify the desired rounding behavior."));
	}
}
